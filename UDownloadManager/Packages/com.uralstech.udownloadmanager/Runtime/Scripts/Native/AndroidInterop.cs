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
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Android;

#nullable enable
namespace Uralstech.UDownloadManager.Native
{
    /// <summary>Interface for the native Android plugin.</summary>
    public static class AndroidInterop
    {
        /// <summary>Possible statuses for a download.</summary>
        [Flags]
        public enum DownloadStatus
        {
            /// <summary>None, invalid status.</summary>
            None = 0,
        
            /// <summary>The download is waiting to start.</summary>
            Pending = 1,
        
            /// <summary>The download is currently running.</summary>
            Running = 2,
        
            /// <summary>The download is waiting to retry or resume.</summary>
            Paused = 4,
        
            /// <summary>The download has successfully completed.</summary>
            Successful = 8,
        
            /// <summary>The download has failed (and will not be retried).</summary>
            Failed = 16,
        }

        /// <summary>Converts an Android download status to a global download status.</summary>
        public static Uralstech.UDownloadManager.DownloadStatus ToDownloadStatus(this DownloadStatus status) =>
            (Uralstech.UDownloadManager.DownloadStatus)status;

        /// <summary>Converts a global download status to an Android download status.</summary>
        public static DownloadStatus ToAndroidDownloadStatus(this Uralstech.UDownloadManager.DownloadStatus status) =>
            (DownloadStatus)status;
        
        /// <summary>The main native interface.</summary>
        public static class DownloadManagerInterface
        {
            /// <summary>Handles callbacks from the native interface.</summary>
            public sealed class Callbacks : AndroidJavaProxy
            {
                private const string ClassName = "com.uralstech.udownloadmanager.DownloadManagerInterface$Callbacks";

                /// <summary>
                /// Broadcast intent action sent by the download manager when a download completes,
                /// along with the download ID.
                /// </summary>
                public event Action<long>? OnDownloadCompleted;

                /// <summary>
                /// Broadcast intent action sent by the download manager when the user clicks on a
                /// running download, either from a system notification or from the downloads UI,
                /// along with the download IDs.
                /// </summary>
                public event Action<long[]>? OnDownloadNotificationClicked;

                /// <summary>
                /// Intent action to launch an activity to display all downloads,
                /// along with a boolean indicating if they should be sorted by size.
                /// </summary>
                public event Action<bool>? OnViewDownloads;
                
                /// <exception cref="PlatformNotSupportedException">Thrown if this constructor is called on a runtime other than Android.</exception>
#if UNITY_ANDROID && !UNITY_EDITOR
                public Callbacks() : base(ClassName) { }
#else
                public Callbacks() : base((AndroidJavaClass)null!)
                {
                    throw new PlatformNotSupportedException();
                }
#endif

                [UnityEngine.Scripting.Preserve]
                [SuppressMessage("ReSharper", "InconsistentNaming")]
                private void onDownloadCompleted(long id) =>
                    TryCall(OnDownloadCompleted, id);
                
                [UnityEngine.Scripting.Preserve]
                [SuppressMessage("ReSharper", "InconsistentNaming")]
                private void onDownloadNotificationClicked(long[] ids) =>
                    TryCall(OnDownloadNotificationClicked, ids);

                [UnityEngine.Scripting.Preserve]
                [SuppressMessage("ReSharper", "InconsistentNaming")]
                private void onViewDownloads(bool sortBySize) =>
                    TryCall(OnViewDownloads, sortBySize);
                
                private void TryCall<T>(Action<T>? action, T param)
                {
                    try
                    {
                        action?.Invoke(param);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }

            /// <summary>Gets an instance of the native interface.</summary>
            /// <remarks>
            /// <para>
            /// This method should generally be called only once during the application's lifetime,
            /// as the native object is supposed to act like a singleton. If you create a new instance
            /// of the native object while another one is active, all unfulfilled callbacks meant for
            /// the existing instance will be forwarded to the new instance's <paramref name="callbacks"/> instead.
            /// </para>
            /// <para>
            /// The returned <see cref="AndroidJavaObject"/> should be disposed when no longer needed.
            /// </para>
            /// </remarks>
            /// <returns>A new instance of the native interface.</returns>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static AndroidJavaObject CreateInstance(Callbacks callbacks)
            {
                ThrowIfNotAndroid();
            
                const string ClassName = "com.uralstech.udownloadmanager.DownloadManagerInterface";
                using AndroidJavaClass nativeInterfaceClass = new(ClassName);
            
                return nativeInterfaceClass.CallStatic<AndroidJavaObject>("createInstance",
                    AndroidApplication.currentContext, callbacks);
            }

            /// <summary>
            /// Returns maximum size, in bytes, of downloads that may go over a
            /// mobile connection; or <see langword="null"/> if there's no limit.
            /// </summary>
            /// <param name="native">The native plugin instance.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static long? GetMaxBytesOverMobile(AndroidJavaObject native)
            {
                ThrowIfNotAndroid();
                
                using AndroidJavaObject? val = native.Call<AndroidJavaObject>(
                    "getMaxBytesOverMobile");
                return val?.Call<long>("longValue");
            }

            /// <summary>
            /// Returns recommended maximum size, in bytes, of downloads that may
            /// go over a mobile connection; or <see langword="null"/> if there's
            /// no recommended limit. The user will have the option to bypass this limit.
            /// </summary>
            /// <param name="native">The native plugin instance.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static long? GetRecommendedMaxBytesOverMobile(AndroidJavaObject native)
            {
                ThrowIfNotAndroid();
                
                using AndroidJavaObject? val = native.Call<AndroidJavaObject>(
                    "getRecommendedMaxBytesOverMobile");
                return val?.Call<long>("longValue");
            }

            /// <summary>
            /// Enqueue a new download. The download will start automatically once the
            /// download manager is ready to execute it and connectivity is available.
            /// </summary>
            /// <param name="native">The native plugin instance.</param>
            /// <param name="request">A <c>DownloadManager.Request</c> object.</param>
            /// <returns>
            /// An ID for the download, unique across the system. This ID is used to make
            /// future calls related to this download. Returns -1 if the operation fails.
            /// </returns>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static long Enqueue(AndroidJavaObject native, AndroidJavaObject request)
            {
                ThrowIfNotAndroid();
                return native.Call<long>("enqueue", request);
            }

            /// <summary>
            /// Cancel downloads and remove them from the download manager.
            /// Each download will be stopped if it was running, and it will no longer
            /// be accessible through the download manager. If there is a downloaded
            /// file, partial or complete, it is deleted.
            /// </summary>
            /// <param name="native">The native plugin instance.</param>
            /// <param name="ids">The IDs of the downloads to remove.</param>
            /// <returns>The number of downloads actually removed.</returns>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static int Remove(AndroidJavaObject native, long[] ids)
            {
                ThrowIfNotAndroid();
                return native.Call<int>("remove", ids);
            }

            /// <summary>
            /// Query the download manager about downloads that have been requested.
            /// </summary>
            /// <param name="native">The native plugin instance.</param>
            /// <param name="ids">
            /// If not <see langword="null"/> or empty,
            /// include only the downloads with the given IDs.
            /// </param>
            /// <param name="status">
            /// If not <see cref="DownloadStatus.None"/>,
            /// include only downloads with status matching any
            /// of the given status flags.
            /// </param>
            /// <returns>A <c>Cursor</c> object.</returns>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static AndroidJavaObject Query(AndroidJavaObject native,
                long[]? ids = null, DownloadStatus status = DownloadStatus.None)
            {
                ThrowIfNotAndroid();
                return native.Call<AndroidJavaObject>("query",
                    ids ?? Array.Empty<long>(), (int)status);
            }
        }
        
        /// <summary>Interface for <a href="https://developer.android.com/reference/android/app/DownloadManager.Request"><c>DownloadManager.Request</c></a>.</summary>
        public static class DownloadManagerRequest
        {
            /// <summary>Creates a new Request object.</summary>
            /// <remarks>The returned <see cref="AndroidJavaObject"/> should be disposed when no longer needed.</remarks>
            /// <param name="uri">The HTTP or HTTPS URI to download.</param>
            /// <returns>A new instance of Request.</returns>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static AndroidJavaObject CreateInstance(AndroidJavaObject uri)
            {
                ThrowIfNotAndroid();
                
                const string ClassName = "android.app.DownloadManager$Request";
                return new AndroidJavaObject(ClassName, uri);
            }

            /// <summary>
            /// Add an HTTP header to be included with the download request.
            /// The header will be added to the end of the list.
            /// </summary>
            /// <param name="native">The native object.</param>
            /// <param name="header">HTTP header name</param>
            /// <param name="value">Header value.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static void AddRequestHeader(AndroidJavaObject native,
                string header, string value)
            {
                ThrowIfNotAndroid();
                using AndroidJavaObject _ =
                    native.Call<AndroidJavaObject>("addRequestHeader", header, value);
            }

            /// <summary>
            /// Restrict the types of networks over which this download may proceed.
            /// By default, all network types are allowed. Consider using
            /// <see cref="SetAllowedOverMetered"/> instead, since it's more flexible.
            /// </summary>
            /// <remarks>
            /// As of Android 7.0, setting only the <see cref="NetworkTypes.WiFi"/>
            /// flag here is equivalent to calling <see cref="SetAllowedOverMetered"/>
            /// with <see langword="false"/>.
            /// </remarks>
            /// <param name="native">The native object.</param>
            /// <param name="networkTypes">Any combination of <see cref="NetworkTypes"/>.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static void SetAllowedNetworkTypes(AndroidJavaObject native, NetworkTypes networkTypes)
            {
                ThrowIfNotAndroid();
                using AndroidJavaObject _ =
                    native.Call<AndroidJavaObject>("setAllowedNetworkTypes", (int)networkTypes);
            }

            /// <summary>
            /// Set whether this download may proceed over a metered network connection.
            /// By default, metered networks are allowed.
            /// </summary>
            /// <param name="native">The native object.</param>
            /// <param name="allow">Whether this download may proceed over a metered connection.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static void SetAllowedOverMetered(AndroidJavaObject native, bool allow)
            {
                ThrowIfNotAndroid();
                using AndroidJavaObject _ =
                    native.Call<AndroidJavaObject>("setAllowedOverMetered", allow);
            }
            
            /// <summary>
            /// Set whether this download may proceed over a roaming connection.
            /// By default, roaming is allowed.
            /// </summary>
            /// <param name="native">The native object.</param>
            /// <param name="allow">Whether to allow a roaming connection to be used.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static void SetAllowedOverRoaming(AndroidJavaObject native, bool allow)
            {
                ThrowIfNotAndroid();
                using AndroidJavaObject _ =
                    native.Call<AndroidJavaObject>("setAllowedOverRoaming", allow);
            }

            /// <summary>
            /// Set the title of this download, to be displayed in notifications (if enabled).
            /// If no title is given, a default one will be assigned based on the download
            /// filename, once the download starts.
            /// </summary>
            /// <param name="native">The native object.</param>
            /// <param name="title">The title.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static void SetTitle(AndroidJavaObject native, string title)
            {
                ThrowIfNotAndroid();
                using AndroidJavaObject _ =
                    native.Call<AndroidJavaObject>("setTitle", title);
            }
            
            /// <summary>
            /// Set a description of this download, to be displayed in notifications (if enabled).
            /// </summary>
            /// <param name="native">The native object.</param>
            /// <param name="description">The description.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static void SetDescription(AndroidJavaObject native, string description)
            {
                ThrowIfNotAndroid();
                using AndroidJavaObject _ =
                    native.Call<AndroidJavaObject>("setDescription", description);
            }

            /// <summary>
            /// Set the local destination for the downloaded file to a path within the application's
            /// external files directory (as returned by <c>Context.getExternalFilesDir(String)</c>,
            /// aka <see cref="Application.persistentDataPath"/>).
            /// </summary>
            /// <param name="native">The native object.</param>
            /// <param name="subPath">The path within the external directory, including the destination filename.</param>
            /// <param name="dirType">The directory type to pass to <c>Context.getExternalFilesDir(String)</c></param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static void SetDestinationInExternalFilesDir(AndroidJavaObject native,
                string subPath, string? dirType = null)
            {
                ThrowIfNotAndroid();
                using AndroidJavaObject _ =
                    native.Call<AndroidJavaObject>("setDestinationInExternalFilesDir",
                        AndroidApplication.currentContext, dirType, subPath);
            }

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
            /// <param name="native">The native object.</param>
            /// <param name="subPath">The path within the external directory, including the destination filename.</param>
            /// <param name="dirType">The directory type to pass to <c>Environment.getExternalStoragePublicDirectory(String)</c></param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static void SetDestinationInExternalPublicDir(AndroidJavaObject native,
                string subPath, string dirType)
            {
                ThrowIfNotAndroid();
                using AndroidJavaObject _ =
                    native.Call<AndroidJavaObject>("setDestinationInExternalPublicDir", dirType, subPath);
            }

            /// <summary>
            /// Set the local destination for the downloaded file. Must be a file URI to a path on external storage,
            /// and the calling application must have the WRITE_EXTERNAL_STORAGE permission.
            /// </summary>
            /// <remarks>
            /// <para>
            /// By default, downloads are saved to a generated filename in the shared download cache and may be
            /// deleted by the system at any time to reclaim space.
            /// </para>
            /// <para>
            /// For applications targeting Android 10 or above, WRITE EXTERNAL_STORAGE permission is not needed
            /// and the <paramref name="uri"/> must refer to a path within the directories owned by the application
            /// (e.g. <c>Context.getExternalFilesDir(String)</c>) or a path within the top-level Downloads directory
            /// (as returned by <c>Environment.getExternalStoragePublicDirectory(String)</c> with <c>Environment.DIRECTORY_DOWNLOADS</c>).
            /// All non-visible downloads that are not modified in the last 7 days will be deleted during idle runs.
            /// </para>
            /// </remarks>
            /// <param name="native">The native object.</param>
            /// <param name="uri">A file Uri indicating the destination for the downloaded file.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static void SetDestinationUri(AndroidJavaObject native, AndroidJavaObject uri)
            {
                ThrowIfNotAndroid();
                using AndroidJavaObject _ =
                    native.Call<AndroidJavaObject>("setDestinationUri", uri);
            }
            
            /// <summary>
            /// Set the MIME content type of this download.
            /// This will override the content type declared in the server's response.
            /// </summary>
            /// <param name="native">The native object.</param>
            /// <param name="mimeType">The MIME type.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static void SetMimeType(AndroidJavaObject native, string mimeType)
            {
                ThrowIfNotAndroid();
                using AndroidJavaObject _ =
                    native.Call<AndroidJavaObject>("setMimeType", mimeType);
            }

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
            /// <param name="native">The native object.</param>
            /// <param name="visibility">The notification visibility.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static void SetNotificationVisibility(AndroidJavaObject native,
                NotificationVisibility visibility)
            {
                ThrowIfNotAndroid();
                using AndroidJavaObject _ =
                    native.Call<AndroidJavaObject>("setNotificationVisibility", (int)visibility);
            }

            /// <summary>
            /// Specify that to run this download, the device needs to be plugged in.
            /// This defaults to <see langword="false"/>.
            /// </summary>
            /// <param name="native">The native object.</param>
            /// <param name="requiresCharging">Whether the device is plugged in.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static void SetRequiresCharging(AndroidJavaObject native,
                bool requiresCharging)
            {
                ThrowIfNotAndroid();
                using AndroidJavaObject _ =
                    native.Call<AndroidJavaObject>("setRequiresCharging", requiresCharging);
            }
            
            /// <summary>
            /// Specify that to run, the download needs the device to be in idle mode.
            /// This defaults to <see langword="false"/>.
            /// </summary>
            /// <remarks>
            /// Idle mode is a loose definition provided by the system, which means that
            /// the device is not in use, and has not been in use for some time.
            /// </remarks>
            /// <param name="native">The native object.</param>
            /// <param name="requiresDeviceIdle">Whether the device need be within an idle maintenance window.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static void SetRequiresDeviceIdle(AndroidJavaObject native,
                bool requiresDeviceIdle)
            {
                ThrowIfNotAndroid();
                using AndroidJavaObject _ =
                    native.Call<AndroidJavaObject>("setRequiresDeviceIdle", requiresDeviceIdle);
            }
        }
        
        /// <summary>Partial interface for <a href="https://developer.android.com/reference/android/database/Cursor"><c>Cursor</c></a>.</summary>
        public static class DownloadCursor
        {
            /// <summary>Possible columns for the cursor.</summary>
            public static class Columns
            {
                /// <summary>Number of bytes download so far.</summary>
                public const string BytesDownloadedSoFar = "bytes_so_far";
                
                /// <summary>
                /// Total size of the download in bytes. This will initially be -1
                /// and will be filled in once the download starts.
                /// </summary>
                public const string TotalSizeBytes = "total_size";
                
                /// <summary>
                /// The client-supplied title for this download.
                /// This will be displayed in system notifications.
                /// Defaults to the empty string.
                /// </summary>
                public const string Title = "title";
                
                /// <summary>
                /// The client-supplied description of this download.
                /// This will be displayed in system notifications.
                /// Defaults to the empty string.
                /// </summary>
                public const string Description = "description";
                
                /// <summary>
                /// An identifier for a particular download, unique across the system.
                /// Clients use this ID to make subsequent calls related to the download.
                /// </summary>
                public const string Id = "_id";
                
                /// <summary>
                /// Timestamp when the download was last modified,
                /// in <c>System.currentTimeMillis()</c> (wall clock time in UTC).
                /// </summary>
                public const string LastModifiedTimestamp = "last_modified_timestamp";
                
                /// <summary>
                /// Uri where downloaded file will be stored. If a destination is supplied
                /// by client, that URI will be used here. Otherwise, the value will initially
                /// be null and will be filled in with a generated URI once the download has started.
                /// </summary>
                public const string LocalUri = "local_uri";
                
                /// <summary>URI to be downloaded.</summary>
                public const string Uri = "uri";
                
                /// <summary>
                /// Internet Media Type of the downloaded file. If no value is provided upon creation,
                /// this will initially be null and will be filled in based on the server's response
                /// once the download has started.
                /// </summary>
                public const string MediaType = "media_type";
                
                /// <summary>
                /// Provides more detail on the status of the download. Its meaning depends on the value
                /// of <see cref="Status"/>. When <see cref="Status"/> is <see cref="DownloadStatus.Failed"/>,
                /// this indicates the type of error that occurred. If an HTTP error occurred, this will hold
                /// the HTTP status code as defined in RFC 2616. Otherwise, it will hold one of <see cref="DownloadFailReason"/>.
                /// When <see cref="Status"/> is <see cref="DownloadStatus.Paused"/>, this indicates why the download
                /// is paused. It will hold one of <see cref="DownloadPauseReason"/>. If <see cref="Status"/> is neither
                /// <see cref="DownloadStatus.Failed"/> nor <see cref="DownloadStatus.Paused"/>, this column's value is undefined.
                /// </summary>
                public const string Reason = "reason";
                
                /// <summary>
                /// Current status of the download, as one of <see cref="DownloadStatus"/>.
                /// </summary>
                public const string Status = "status";
            }

            /// <summary>Data type of column field.</summary>
            public enum FieldType
            {
                Blob = 4,
                Float = 2,
                Integer = 1,
                Null = 0,
                String = 3,
            }
            
            /// <summary>Move the cursor to the first row.</summary>
            /// <remarks>This method will return <see langword="false"/> if the cursor is empty.</remarks>
            /// <param name="native">The native object.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static bool MoveToFirst(AndroidJavaObject native)
            {
                ThrowIfNotAndroid();
                return native.Call<bool>("moveToFirst");
            }
            
            /// <summary>Move the cursor to the next row.</summary>
            /// <remarks>
            /// This method will return <see langword="false"/> if the cursor is
            /// already past the last entry in the result set.
            /// </remarks>
            /// <param name="native">The native object.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static bool MoveToNext(AndroidJavaObject native)
            {
                ThrowIfNotAndroid();
                return native.Call<bool>("moveToNext");
            }

            /// <summary>Returns the zero-based index for the given column name, or -1 if the column doesn't exist.</summary>
            /// <param name="native">The native object.</param>
            /// <param name="columnName">The column name.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static int GetColumnIndex(AndroidJavaObject native, string columnName)
            {
                ThrowIfNotAndroid();
                return native.Call<int>("getColumnIndex", columnName);
            }

            /// <summary>
            /// Returns data type of the given column's value. The preferred type of
            /// the column is returned but the data may be converted to other types
            /// as documented in the get-type methods such as <see cref="GetInt"/>,
            /// <see cref="GetFloat"/> etc.
            /// </summary>
            /// <param name="native">The native object.</param>
            /// <param name="columnIndex">The zero-based index of the target column.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static FieldType GetType(AndroidJavaObject native, int columnIndex)
            {
                ThrowIfNotAndroid();
                return native.Call<FieldType>("getType", columnIndex);
            }

            /// <summary>Returns the value of the requested column as a byte array.</summary>
            /// <param name="native">The native object.</param>
            /// <param name="columnIndex">The zero-based index of the target column.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static sbyte[] GetBlob(AndroidJavaObject native, int columnIndex)
            {
                ThrowIfNotAndroid();
                return native.Call<sbyte[]>("getBlob", columnIndex);
            }
            
            /// <summary>Returns the value of the requested column as a double.</summary>
            /// <remarks>
            /// The result and whether this method throws an exception when the column value
            /// is <see langword="null"/>, the column type is not a floating-point type, or
            /// the floating-point value is not representable as a double value is
            /// implementation-defined.
            /// </remarks>
            /// <param name="native">The native object.</param>
            /// <param name="columnIndex">The zero-based index of the target column.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static double GetDouble(AndroidJavaObject native, int columnIndex)
            {
                ThrowIfNotAndroid();
                return native.Call<double>("getDouble", columnIndex);
            }
            
            /// <summary>Returns the value of the requested column as a float.</summary>
            /// <remarks>
            /// The result and whether this method throws an exception when the column value
            /// is <see langword="null"/>, the column type is not a floating-point type, or
            /// the floating-point value is not representable as a float value is
            /// implementation-defined.
            /// </remarks>
            /// <param name="native">The native object.</param>
            /// <param name="columnIndex">The zero-based index of the target column.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static float GetFloat(AndroidJavaObject native, int columnIndex)
            {
                ThrowIfNotAndroid();
                return native.Call<float>("getFloat", columnIndex);
            }

            /// <summary>Returns the value of the requested column as an int.</summary>
            /// <remarks>
            /// The result and whether this method throws an exception when the column value
            /// is <see langword="null"/>, the column type is not an integral type, or the
            /// integer value is outside the range [<c>Integer.MIN_VALUE</c>, <c>Integer.MAX_VALUE</c>]
            /// is implementation-defined.
            /// </remarks>
            /// <param name="native">The native object.</param>
            /// <param name="columnIndex">The zero-based index of the target column.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static int GetInt(AndroidJavaObject native, int columnIndex)
            {
                ThrowIfNotAndroid();
                return native.Call<int>("getInt", columnIndex);
            }

            /// <summary>Returns the value of the requested column as a short.</summary>
            /// <remarks>
            /// The result and whether this method throws an exception when the column value
            /// is <see langword="null"/>, the column type is not an integral type, or the
            /// integer value is outside the range [<c>Short.MIN_VALUE</c>, <c>Short.MAX_VALUE</c>]
            /// is implementation-defined.
            /// </remarks>
            /// <param name="native">The native object.</param>
            /// <param name="columnIndex">The zero-based index of the target column.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static short GetShort(AndroidJavaObject native, int columnIndex)
            {
                ThrowIfNotAndroid();
                return native.Call<short>("getShort", columnIndex);
            }

            /// <summary>Returns the value of the requested column as a long.</summary>
            /// <remarks>
            /// The result and whether this method throws an exception when the column value
            /// is <see langword="null"/>, the column type is not an integral type, or the
            /// integer value is outside the range [<c>Long.MIN_VALUE</c>, <c>Long.MAX_VALUE</c>]
            /// is implementation-defined.
            /// </remarks>
            /// <param name="native">The native object.</param>
            /// <param name="columnIndex">The zero-based index of the target column.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static long GetLong(AndroidJavaObject native, int columnIndex)
            {
                ThrowIfNotAndroid();
                return native.Call<long>("getLong", columnIndex);
            }

            /// <summary>Returns the value of the requested column as a string.</summary>
            /// <remarks>
            /// The result and whether this method throws an exception when the column value
            /// is <see langword="null"/> or the column type is not a string type is
            /// implementation-defined.
            /// </remarks>
            /// <param name="native">The native object.</param>
            /// <param name="columnIndex">The zero-based index of the target column.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static string GetString(AndroidJavaObject native, int columnIndex)
            {
                ThrowIfNotAndroid();
                return native.Call<string>("getString", columnIndex);
            }
            
            /// <summary>Returns <see langword="true"/> if the value in the indicated column is <see langword="null"/>.</summary>
            /// <param name="native">The native object.</param>
            /// <param name="columnIndex">The zero-based index of the target column.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static bool IsNull(AndroidJavaObject native, int columnIndex)
            {
                ThrowIfNotAndroid();
                return native.Call<bool>("isNull", columnIndex);
            }

            /// <summary>Returns <see langword="true"/> if the cursor is closed.</summary>
            /// <param name="native">The native object.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static bool IsClosed(AndroidJavaObject native)
            {
                ThrowIfNotAndroid();
                return native.Call<bool>("isClosed");
            }
            
            /// <summary>Closes the Cursor, releasing all of its resources and making it completely invalid.</summary>
            /// <param name="native">The native object.</param>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static void Close(AndroidJavaObject native)
            {
                ThrowIfNotAndroid();
                native.Call("close");
            }
        }
        
        /// <summary>Partial interface for <a href="https://developer.android.com/reference/android/net/Uri"><c>Uri</c></a>.</summary>
        public static class Uri
        {
            /// <summary>Creates a new Uri object.</summary>
            /// <remarks>The returned <see cref="AndroidJavaObject"/> should be disposed when no longer needed.</remarks>
            /// <param name="uri">The HTTP or HTTPS URI.</param>
            /// <returns>A new instance of Uri.</returns>
            /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
            public static AndroidJavaObject CreateInstance(string uri)
            {
                ThrowIfNotAndroid();

                const string ClassName = "android.net.Uri";
                using AndroidJavaClass @class = new(ClassName);
                return @class.CallStatic<AndroidJavaObject>("parse", uri);
            }
        }
        
        private static void ThrowIfNotAndroid()
        {
#if !UNITY_ANDROID || UNITY_EDITOR
            throw new PlatformNotSupportedException();
#endif
        }
    }
}