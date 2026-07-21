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

#nullable enable
namespace Uralstech.UDownloadManager
{
    /// <summary>Possible statuses for a download.</summary>
    [Flags]
    public enum DownloadStatus
    {
        /// <summary>The download is waiting to start.</summary>
        Pending     = 1 << 0,
        
        /// <summary>The download is currently running.</summary>
        Running     = 1 << 1,
        
        /// <summary>The download is waiting to retry or resume.</summary>
        Paused      = 1 << 2,
        
        /// <summary>The download has successfully completed.</summary>
        Successful  = 1 << 3,
        
        /// <summary>The download has failed (and will not be retried).</summary>
        Failed      = 1 << 4,
    }
    
    /// <summary>Download failure reasons.</summary>
    public enum DownloadFailReason
    {
        /// <summary>Some possibly transient error occurred but we can't resume the download.</summary>
        CannotResume = 1008,
        
        /// <summary>No external storage device was found. Typically, this is because the SD card is not mounted.</summary>
        DeviceNotFound = 1007,
        
        /// <summary>The requested destination file already exists (the download manager will not overwrite an existing file).</summary>
        FileAlreadyExists = 1009,
        
        /// <summary>A storage issue arose which doesn't fit under any other error code.</summary>
        FileError = 1001,
        
        /// <summary>An error receiving or processing data occurred at the HTTP level.</summary>
        HttpDataError = 1004,

        /// <summary>There was insufficient storage space. Typically, this is because the SD card is full.</summary>
        InsufficientSpace = 1006,
        
        /// <summary>There were too many redirects.</summary>
        TooManyRedirects = 1005,
        
        /// <summary>An HTTP code was received that download manager can't handle.</summary>
        UnhandledHttpCode = 1002,
        
        /// <summary>The download has completed with an error that doesn't fit under any other error code.</summary>
        Unknown = 1000,
    }

    /// <summary>Download pause reasons.</summary>
    public enum DownloadPauseReason
    {
        /// <summary>
        /// The download exceeds a size limit for downloads over the mobile network
        /// and the download manager is waiting for a Wi-Fi connection to proceed.
        /// </summary>
        QueuedForWiFi = 3,
        
        /// <summary>The download is paused for some other reason.</summary>
        Unknown = 4,
        
        /// <summary>The download is waiting for network connectivity to proceed.</summary>
        WaitingForNetwork = 2,
        
        /// <summary>
        /// The download is paused because some network error occurred and the
        /// download manager is waiting before retrying the request.
        /// </summary>
        WaitingToRetry = 1,
    }
    
    /// <summary>Allowed network types for a download.</summary>
    [Flags]
    public enum NetworkTypes
    {
        /// <summary>A Mobile data connection.</summary>
        Mobile = 1,
                
        /// <summary>A Wi-Fi data connection.</summary>
        WiFi = 2,
    }

    /// <summary>Download notification visibility settings.</summary>
    public enum NotificationVisibility
    {
        /// <summary>This download is visible but only shows in the notifications while it's in progress.</summary>
        Visible = 0,
                
        /// <summary>This download is visible and shows in the notifications while in progress and after completion.</summary>
        VisibleNotifyCompleted = 1,
                
        /// <summary>This download doesn't show in the UI or in the notifications.</summary>
        Hidden = 2,
    }
}