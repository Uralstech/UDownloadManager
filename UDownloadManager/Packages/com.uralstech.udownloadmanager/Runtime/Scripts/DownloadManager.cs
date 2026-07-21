// Copyright 2026 URAV ADVANCED LEARNING SYSTEMS PRIVATE LIMITED
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using Uralstech.UDownloadManager.Native;
using Uralstech.Utils.Singleton;

#nullable enable
namespace Uralstech.UDownloadManager
{
    [AddComponentMenu("Uralstech/UDownloadManager/DownloadManager")]
    public sealed class DownloadManager : Singleton<DownloadManager>
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        private static readonly bool s_isAndroid = true;
#else
        private static readonly bool s_isAndroid = false;
#endif

        /// <summary>Invoked when a download has completed, with its ID.</summary>
        public event Action<DownloadId>? OnDownloadCompleted;
        
        /// <summary>
        /// Invoked when the user has clicked on a download notification,
        /// with the ID of the download(s) the notification was for.
        /// </summary>
        public event Action<DownloadId[]>? OnDownloadNotificationClicked;
        
        /// <summary>
        /// Invoked when the user has requested to see all downloads,
        /// with a boolean indicating if they should be sorted by size.
        /// </summary>
        public event Action<bool>? OnViewDownloads;
        
        private AndroidInterop.DownloadManagerInterface.Callbacks? _androidCallbacks;
        private AndroidJavaObject? _androidNative;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (!s_isAndroid) return;
            
            _androidCallbacks = new AndroidInterop.DownloadManagerInterface.Callbacks();
            _androidCallbacks.OnDownloadCompleted += OnDownloadCompleteAndroid;
            _androidCallbacks.OnDownloadNotificationClicked += OnDownloadNotificationClickedAndroid;
            _androidCallbacks.OnViewDownloads += OnViewDownloadsAndroid;
                
            _androidNative = AndroidInterop.DownloadManagerInterface.CreateInstance(_androidCallbacks);
        }

        private void OnDestroy()
        {
            if (!s_isAndroid) return;
            _androidNative?.Dispose();
            
            if (_androidCallbacks == null) return;
            _androidCallbacks.OnDownloadCompleted -= OnDownloadCompleteAndroid;
            _androidCallbacks.OnDownloadNotificationClicked -= OnDownloadNotificationClickedAndroid;
            _androidCallbacks.OnViewDownloads -= OnViewDownloadsAndroid;
        }

        /// <summary>
        /// Returns the maximum size of a download performed with
        /// a mobile connection, or -1 if there is no defined limit.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on an unsupported platform.</exception>
        public long GetMobileDownloadSizeLimit()
        {
            if (s_isAndroid)
                return AndroidInterop.DownloadManagerInterface.GetMaxBytesOverMobile(_androidNative!) ?? -1;

            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Returns the recommended maximum size of a download performed with
        /// a mobile connection, or -1 if there is no defined limit.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on an unsupported platform.</exception>
        public long GetRecommendedMobileDownloadSizeLimit()
        {
            if (s_isAndroid)
                return AndroidInterop.DownloadManagerInterface.GetRecommendedMaxBytesOverMobile(_androidNative!) ?? -1;

            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// Tries to enqueue a new download. The download will start automatically once the
        /// download manager is ready to execute it and connectivity is available.
        /// </summary>
        /// <param name="request">The request. Must be of type <see cref="AndroidDownloadRequest"/> on Android.</param>
        /// <param name="id">
        /// An ID for the download, unique across the system. This ID is used to make
        /// future calls related to this download. Returns -1 if the operation fails.
        /// </param>
        /// <returns><see langword="true"/> if successful, <see langword="false"/> otherwise.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on an unsupported platform.</exception>
        public bool TryEnqueueDownload(DownloadRequest request, [NotNullWhen(true)] out DownloadId? id)
        {
            if (!s_isAndroid) throw new PlatformNotSupportedException();
            
            if (request is not AndroidDownloadRequest androidRequest)
                throw new ArgumentException($"Request must be of type {nameof(AndroidDownloadRequest)} on Android.", nameof(request));
            
            long androidId = AndroidInterop.DownloadManagerInterface.Enqueue(_androidNative!, androidRequest._native);
            if (androidId == -1)
            {
                id = null;
                return false;
            }
                
            id = DownloadId.FromAndroidId(androidId);
            return true;
        }

        /// <summary>
        /// Cancel downloads and remove them from the download manager.
        /// Each download will be stopped if it was running, and it will no longer
        /// be accessible through the download manager. If there is a downloaded
        /// file, partial or complete, it is deleted.
        /// </summary>
        /// <param name="ids">The IDs of the downloads to remove.</param>
        /// <returns>The number of downloads actually removed.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on an unsupported platform.</exception>
        public int CancelDownloads(params DownloadId[] ids)
        {
            if (s_isAndroid)
                return AndroidInterop.DownloadManagerInterface.Remove(_androidNative!,
                    Array.ConvertAll(ids, static id => id.ToAndroidId()));
            
            throw new PlatformNotSupportedException();
        }

        /// <summary>Query the download manager about downloads that have been requested.</summary>
        /// <param name="filter">An optional filter.</param>
        /// <returns>The downloads.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on an unsupported platform.</exception>
        public IReadOnlyList<Download> QueryDownloads(DownloadFilter? filter = null)
        {
            if (!s_isAndroid) throw new PlatformNotSupportedException();
            
            using AndroidJavaObject cursor = AndroidInterop.DownloadManagerInterface.Query(_androidNative!,
                filter?.Ids, filter?.Status ?? default);

            try
            {
                return AndroidDownload.FromAndroidCursor(cursor);
            }
            finally
            {
                AndroidInterop.DownloadCursor.Close(cursor);
            }
        }
        
        private void OnDownloadCompleteAndroid(long id)
        {
            if (s_isAndroid)
                OnDownloadCompleted?.ForgetOnMainThread(DownloadId.FromAndroidId(id));
        }

        private void OnDownloadNotificationClickedAndroid(long[] ids)
        {
            if (s_isAndroid)
                OnDownloadNotificationClicked?.ForgetOnMainThread(Array.ConvertAll(ids, DownloadId.FromAndroidId));
        }

        private void OnViewDownloadsAndroid(bool sortBySize)
        {
            if (s_isAndroid)
                OnViewDownloads?.ForgetOnMainThread(sortBySize);
        }
    }
}
