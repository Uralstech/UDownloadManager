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

#nullable enable
namespace Uralstech.UDownloadManager
{
    /// <summary>Represents a download.</summary>
    public abstract record Download
    {
        /// <summary>Gets the platform-agnostic identifier of the download.</summary>
        public DownloadId Id { get; protected init; }
    
        /// <summary>Gets the current status of the download.</summary>
        public DownloadStatus Status { get; protected init; }
    
        internal Download()
        {
            Id = null!;
        }
    }
}