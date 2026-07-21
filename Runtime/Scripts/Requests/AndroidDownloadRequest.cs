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
using UnityEngine;
using Uralstech.UDownloadManager.Native;

#nullable enable
namespace Uralstech.UDownloadManager
{
    /// <summary>Download request for Android.</summary>
    public sealed class AndroidDownloadRequest : DownloadRequest
    {
        internal readonly AndroidJavaObject _native;
        private bool _disposed;

        /// <param name="uri">The URI to download from.</param>
        /// <exception cref="PlatformNotSupportedException">Thrown if this constructor is called on a runtime other than Android.</exception>
        public AndroidDownloadRequest(string uri)
        {
            using AndroidJavaObject nativeUri = AndroidInterop.Uri.CreateInstance(uri);
            _native = AndroidInterop.DownloadManagerRequest.CreateInstance(nativeUri);
        }

        /// <summary>
        /// Add an HTTP header to be included with the download request.
        /// The header will be added to the end of the list.
        /// </summary>
        /// <param name="header">HTTP header name</param>
        /// <param name="value">Header value.</param>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
        public void AddRequestHeader(string header, string value) =>
            AndroidInterop.DownloadManagerRequest.AddRequestHeader(_native, header, value);

        /// <summary>
        /// Restrict the types of networks over which this download may proceed.
        /// By default, all network types are allowed. Consider using
        /// <see cref="SetAllowedOverMetered"/> instead, since it's more flexible.
        /// </summary>
        /// <param name="networkTypes">Any combination of <see cref="NetworkTypes"/>.</param>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
        public void SetAllowedNetworkTypes(NetworkTypes networkTypes) =>
            AndroidInterop.DownloadManagerRequest.SetAllowedNetworkTypes(_native, networkTypes);

        /// <summary>
        /// Set whether this download may proceed over a metered network connection.
        /// By default, metered networks are allowed.
        /// </summary>
        /// <param name="allow">Whether this download may proceed over a metered connection.</param>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
        public void SetAllowedOverMetered(bool allow) =>
            AndroidInterop.DownloadManagerRequest.SetAllowedOverMetered(_native, allow);

        /// <summary>
        /// Set whether this download may proceed over a roaming connection.
        /// By default, roaming is allowed.
        /// </summary>
        /// <param name="allow">Whether to allow a roaming connection to be used.</param>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
        public void SetAllowedOverRoaming(bool allow) =>
            AndroidInterop.DownloadManagerRequest.SetAllowedOverRoaming(_native, allow);

        /// <summary>
        /// Set the title of this download, to be displayed in notifications (if enabled).
        /// If no title is given, a default one will be assigned based on the download
        /// filename, once the download starts.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
        public void SetTitle(string title) =>
            AndroidInterop.DownloadManagerRequest.SetTitle(_native, title);

        /// <summary>
        /// Set a description of this download, to be displayed in notifications (if enabled).
        /// </summary>
        /// <param name="description">The description.</param>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
        public void SetDescription(string description) =>
            AndroidInterop.DownloadManagerRequest.SetDescription(_native, description);

        /// <summary>
        /// Set the local destination for the downloaded file to a path within the application's
        /// external files directory (as returned by <c>Context.getExternalFilesDir(String)</c>,
        /// aka <see cref="Application.persistentDataPath"/>).
        /// </summary>
        /// <param name="subPath">The path within the external directory, including the destination filename.</param>
        /// <param name="dirType">The directory type to pass to <c>Context.getExternalFilesDir(String)</c></param>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
        public void SetDestinationInExternalFilesDir(string subPath, string? dirType = null) =>
            AndroidInterop.DownloadManagerRequest.SetDestinationInExternalFilesDir(_native, subPath, dirType);

        /// <summary>
        /// Set the local destination for the downloaded file to a path within the public external
        /// storage directory (as returned by <c>Environment.getExternalStoragePublicDirectory(String)</c>).
        /// </summary>
        /// <remarks>
        /// For applications targeting Android 10 or above, the WRITE_EXTERNAL_STORAGE permission is not
        /// needed and the <paramref name="dirType"/> must be one of the known public directories like
        /// <c>Environment.DIRECTORY_DOWNLOADS</c>, <c>Environment.DIRECTORY_PICTURES</c>,
        /// <c>Environment.DIRECTORY_MOVIES</c>, etc.
        /// </remarks>
        /// <param name="subPath">The path within the external directory, including the destination filename.</param>
        /// <param name="dirType">The directory type to pass to <c>Environment.getExternalStoragePublicDirectory(String)</c></param>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
        public void SetDestinationInExternalPublicDir(string subPath, string dirType) =>
            AndroidInterop.DownloadManagerRequest.SetDestinationInExternalPublicDir(_native, subPath, dirType);

        /// <summary>
        /// Set the destination for the downloaded file. Must be an absolute path on external storage,
        /// and the calling application may need the WRITE_EXTERNAL_STORAGE permission.
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default, downloads are saved to a generated filename in the shared download cache and may be
        /// deleted by the system at any time to reclaim space.
        /// </para>
        /// <para>
        /// For applications targeting Android 10 or above, WRITE EXTERNAL_STORAGE permission is not needed
        /// and the <paramref name="path"/> must refer to a path within the directories owned by the application
        /// (e.g. <see cref="Application.persistentDataPath"/>) or a path within the top-level Downloads directory
        /// (as returned by <c>Environment.getExternalStoragePublicDirectory(String)</c> with <c>Environment.DIRECTORY_DOWNLOADS</c>).
        /// All non-visible downloads that are not modified in the last 7 days will be deleted during idle runs.
        /// </para>
        /// </remarks>
        /// <param name="path">The path.</param>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
        public void SetDestination(string path)
        {
            using AndroidJavaObject uri = AndroidInterop.Uri.CreateInstance($"file:///{path.TrimStart('/')}");
            AndroidInterop.DownloadManagerRequest.SetDestinationUri(_native, uri);
        }

        /// <summary>
        /// Set the MIME content type of this download.
        /// This will override the content type declared in the server's response.
        /// </summary>
        /// <param name="mimeType">The MIME type.</param>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
        public void SetMimeType(string mimeType) =>
            AndroidInterop.DownloadManagerRequest.SetMimeType(_native, mimeType);
    
        /// <summary>
        /// Control whether a system notification is posted by the download manager
        /// while this download is running or when it is completed. If enabled,
        /// the download manager posts notifications about downloads through the
        /// system <c>NotificationManager</c>. By default, a notification is shown
        /// only when the download is in progress.
        /// </summary>
        /// <remarks>
        /// If set to <see cref="NotificationVisibility.Hidden"/>, this requires
        /// the permission <c>android.permission.DOWNLOAD_WITHOUT_NOTIFICATION</c>.
        /// </remarks>
        /// <param name="visibility">The notification visibility.</param>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
        public void SetNotificationVisibility(NotificationVisibility visibility) =>
            AndroidInterop.DownloadManagerRequest.SetNotificationVisibility(_native, visibility);
        
        /// <summary>
        /// Specify that to run this download, the device needs to be plugged in.
        /// This defaults to <see langword="false"/>.
        /// </summary>
        /// <param name="requiresCharging">Whether the device is plugged in.</param>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
        public void SetRequiresCharging(bool requiresCharging) =>
            AndroidInterop.DownloadManagerRequest.SetRequiresCharging(_native, requiresCharging);
        
        /// <summary>
        /// Specify that to run, the download needs the device to be in idle mode.
        /// This defaults to <see langword="false"/>.
        /// </summary>
        /// <remarks>
        /// Idle mode is a loose definition provided by the system, which means that
        /// the device is not in use, and has not been in use for some time.
        /// </remarks>
        /// <param name="requiresDeviceIdle">Whether the device need be within an idle maintenance window.</param>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
        public void SetRequiresDeviceIdle(bool requiresDeviceIdle) =>
            AndroidInterop.DownloadManagerRequest.SetRequiresDeviceIdle(_native, requiresDeviceIdle);
        
        /// <summary>Disposes native Android instance.</summary>
        public override void Dispose()
        {
            if (_disposed) return;

            _native.Dispose();
            _disposed = true;
        }
    }
}