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
using UnityEngine;
using Uralstech.UDownloadManager.Native;

#nullable enable
namespace Uralstech.UDownloadManager
{
    /// <summary>
    /// Represents a download managed by Android's
    /// <c>android.app.DownloadManager</c>.
    /// </summary>
    public sealed record AndroidDownload : Download
    {
        /// <summary>Gets the URI from which the file is being downloaded.</summary>
        public string Uri { get; private init; }
    
        /// <summary>
        /// Gets the local URI where the downloaded file is stored, or will be
        /// stored once the download begins.
        /// </summary>
        public string? Path { get; private init; }
    
        /// <summary>
        /// Gets the Internet media type (MIME type) of the downloaded file, if
        /// known.
        /// </summary>
        public string? MediaType { get; private init; }
    
        /// <summary>
        /// Gets the client-supplied title of the download, if any.
        /// </summary>
        public string Title { get; private init; }
    
        /// <summary>
        /// Gets the client-supplied description of the download, if any.
        /// </summary>
        public string Description { get; private init; }
    
        /// <summary>
        /// Gets the total size of the download, in bytes.
        /// </summary>
        /// <remarks>
        /// A value of <c>-1</c> indicates that the total size is not yet known.
        /// </remarks>
        public long TotalSize { get; private init; }
    
        /// <summary>
        /// Gets the number of bytes downloaded so far.
        /// </summary>
        public long DownloadedSoFar { get; private init; }
    
        /// <summary>
        /// Gets the UTC timestamp when the download was last modified.
        /// </summary>
        public DateTime LastModified { get; private init; }
        
        /// <summary>
        /// Gets the reason the download failed.
        /// </summary>
        /// <remarks>
        /// This value is only meaningful when <see cref="Download.Status"/> is
        /// <see cref="DownloadStatus.Failed"/>.
        /// </remarks>
        public DownloadFailReason FailReason { get; private init; }
    
        /// <summary>
        /// Gets the reason the download is paused.
        /// </summary>
        /// <remarks>
        /// This value is only meaningful when <see cref="Download.Status"/> is
        /// <see cref="DownloadStatus.Paused"/>.
        /// </remarks>
        public DownloadPauseReason PauseReason { get; private init; }

        private AndroidDownload()
        {
            Uri = Title = Description = string.Empty;
        }
    
        /// <summary>
        /// Creates an array of <see cref="AndroidDownload"/> instances from an
        /// Android <c>Cursor</c> returned by
        /// <c>android.app.DownloadManager.Query</c>.
        /// </summary>
        /// <param name="cursor">
        /// The cursor containing one or more download records.
        /// </param>
        /// <returns>
        /// An array containing every download represented by the cursor. If the
        /// cursor contains no rows, an empty array is returned.
        /// </returns>
        /// <exception cref="InvalidOperationException">Thrown if the cursor does not contain one of the required columns.</exception>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
        public static AndroidDownload[] FromAndroidCursor(AndroidJavaObject cursor)
        {
            if (!AndroidInterop.DownloadCursor.MoveToFirst(cursor))
                return Array.Empty<AndroidDownload>();
        
            List<AndroidDownload> downloads = new();
        
            do
            {
                int idIdx = AndroidInterop.DownloadCursor.GetColumnIndex(cursor, AndroidInterop.DownloadCursor.Columns.Id);
                if (idIdx == -1)
                    throw new InvalidOperationException("ID column not found.");
        
                int statusIdx = AndroidInterop.DownloadCursor.GetColumnIndex(cursor, AndroidInterop.DownloadCursor.Columns.Status);
                if (statusIdx == -1)
                    throw new InvalidOperationException("Status column not found.");
        
                long androidId = AndroidInterop.DownloadCursor.GetLong(cursor, idIdx);
                int statusValue = AndroidInterop.DownloadCursor.GetInt(cursor, statusIdx);
        
                int uriIdx = AndroidInterop.DownloadCursor.GetColumnIndex(cursor, AndroidInterop.DownloadCursor.Columns.Uri);
                int pathIdx = AndroidInterop.DownloadCursor.GetColumnIndex(cursor, AndroidInterop.DownloadCursor.Columns.LocalUri);
                int mediaTypeIdx = AndroidInterop.DownloadCursor.GetColumnIndex(cursor, AndroidInterop.DownloadCursor.Columns.MediaType);
                int titleIdx = AndroidInterop.DownloadCursor.GetColumnIndex(cursor, AndroidInterop.DownloadCursor.Columns.Title);
                int descriptionIdx = AndroidInterop.DownloadCursor.GetColumnIndex(cursor, AndroidInterop.DownloadCursor.Columns.Description);
                int totalSizeIdx = AndroidInterop.DownloadCursor.GetColumnIndex(cursor, AndroidInterop.DownloadCursor.Columns.TotalSizeBytes);
                int downloadedSoFarIdx = AndroidInterop.DownloadCursor.GetColumnIndex(cursor, AndroidInterop.DownloadCursor.Columns.BytesDownloadedSoFar);
                int lastModifiedIdx = AndroidInterop.DownloadCursor.GetColumnIndex(cursor, AndroidInterop.DownloadCursor.Columns.LastModifiedTimestamp);
                int reasonIdx = AndroidInterop.DownloadCursor.GetColumnIndex(cursor, AndroidInterop.DownloadCursor.Columns.Reason);
        
                string uri = ReadString(cursor, uriIdx) ?? string.Empty;
                string? path = ReadString(cursor, pathIdx);
                string? mediaType = ReadString(cursor, mediaTypeIdx);
                string title = ReadString(cursor, titleIdx) ?? string.Empty;
                string description = ReadString(cursor, descriptionIdx) ?? string.Empty;
        
                long totalSize = ReadLongOrDefault(cursor, totalSizeIdx, -1L);
                long downloadedSoFar = ReadLongOrDefault(cursor, downloadedSoFarIdx, 0L);
                DateTime lastModified = ReadDateTimeUtcOrDefault(cursor, lastModifiedIdx, default);
        
                AndroidInterop.DownloadStatus status = (AndroidInterop.DownloadStatus)statusValue;
                DownloadFailReason failReason = default;
                DownloadPauseReason pauseReason = default;
        
                if (reasonIdx != -1 && !AndroidInterop.DownloadCursor.IsNull(cursor, reasonIdx))
                {
                    int reasonValue = AndroidInterop.DownloadCursor.GetInt(cursor, reasonIdx);

                    if (status.HasFlag(AndroidInterop.DownloadStatus.Failed))
                        failReason = (DownloadFailReason)reasonValue;
                    else if (status.HasFlag(AndroidInterop.DownloadStatus.Paused))
                        pauseReason = (DownloadPauseReason)reasonValue;
                }
        
                downloads.Add(new AndroidDownload
                {
                    Id = DownloadId.FromAndroidId(androidId),
                    Uri = uri,
                    Path = path,
                    MediaType = mediaType,
                    Title = title,
                    Description = description,
                    TotalSize = totalSize,
                    DownloadedSoFar = downloadedSoFar,
                    LastModified = lastModified,
                    Status = status.ToDownloadStatus(),
                    FailReason = failReason,
                    PauseReason = pauseReason,
                });
            }
            while (AndroidInterop.DownloadCursor.MoveToNext(cursor));
        
            return downloads.ToArray();
        }
        
        private static string? ReadString(AndroidJavaObject cursor, int columnIndex)
        {
            return columnIndex != -1 && !AndroidInterop.DownloadCursor.IsNull(cursor, columnIndex)
                ? AndroidInterop.DownloadCursor.GetString(cursor, columnIndex) : null;
        }
        
        private static long ReadLongOrDefault(AndroidJavaObject cursor, int columnIndex, long defaultValue)
        {
            return columnIndex != -1 && !AndroidInterop.DownloadCursor.IsNull(cursor, columnIndex)
                ? AndroidInterop.DownloadCursor.GetLong(cursor, columnIndex) : defaultValue;
        }
        
        private static DateTime ReadDateTimeUtcOrDefault(AndroidJavaObject cursor, int columnIndex, DateTime defaultValue)
        {
            if (columnIndex == -1 || AndroidInterop.DownloadCursor.IsNull(cursor, columnIndex))
                return defaultValue;
        
            long millis = AndroidInterop.DownloadCursor.GetLong(cursor, columnIndex);
            return DateTimeOffset.FromUnixTimeMilliseconds(millis).UtcDateTime;
        }
    }
}