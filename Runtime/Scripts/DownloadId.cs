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

#nullable enable
namespace Uralstech.UDownloadManager
{
    /// <summary>Platform-agnostic download ID.</summary>
    public sealed record DownloadId
    {
        private readonly long _androidId;
        private DownloadId(long androidId)
        {
            _androidId = androidId;
        }
        
        /// <summary>Gets the Android-native ID.</summary>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
        public long ToAndroidId() => Application.platform == RuntimePlatform.Android
            ? _androidId : throw new PlatformNotSupportedException();

        /// <summary>Creates a <see cref="DownloadId"/> from an Android-native ID.</summary>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a runtime other than Android.</exception>
        public static DownloadId FromAndroidId(long id) => Application.platform == RuntimePlatform.Android
            ? new DownloadId(id) : throw new PlatformNotSupportedException();
    }
}