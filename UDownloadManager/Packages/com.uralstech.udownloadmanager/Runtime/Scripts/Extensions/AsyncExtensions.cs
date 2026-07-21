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
    internal static class AsyncExtensions
    {
        public static async void ForgetOnMainThread<T>(this Action<T> action, T arg)
        {
            try
            {
                await Awaitable.MainThreadAsync();
                action.Invoke(arg);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}