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
using Uralstech.UDownloadManager.Native;

#nullable enable
namespace Uralstech.UDownloadManager
{
    /// <summary>(Android) Filters the downloads list.</summary>
    public sealed class DownloadFilter
    {
        internal long[]? Ids { get; private set; }
        internal AndroidInterop.DownloadStatus Status { get; private set; }
        
        /// <summary>Filters by the given download IDs.</summary>
        public void ByIds(params DownloadId[] ids) =>
            Ids = Array.ConvertAll(ids, static id => id.ToAndroidId());
        
        /// <summary>Filters by the given download status.</summary>
        public void ByStatus(DownloadStatus status) =>
            Status = status.ToAndroidDownloadStatus();
    }
}