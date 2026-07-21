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

package com.uralstech.udownloadmanager

import android.app.DownloadManager
import android.content.Context
import android.database.Cursor
import android.net.Uri
import android.util.Log
import java.lang.ref.WeakReference

class DownloadManagerInterface private constructor(private val context: Context, internal val callbacks: Callbacks) {

    companion object {
        internal const val TAG = "UDownloadManager.Native"

        internal var instance: WeakReference<DownloadManagerInterface> = WeakReference(null)
            private set

        @JvmStatic
        fun createInstance(context: Context, callbacks: Callbacks) : DownloadManagerInterface {

            if (instance.get() != null) {
                Log.w(TAG, "Creating new DownloadManagerInterface instance due to getInstance() call. " +
                        "This will redirect all previous unfulfilled callbacks to the new caller.")
            }

            return DownloadManagerInterface(context, callbacks).also {
                instance = WeakReference(it)
            }
        }
    }

    interface Callbacks {
        fun onDownloadCompleted(id: Long)
        fun onDownloadNotificationClicked(ids: LongArray)
        fun onViewDownloads(sortBySize: Boolean)
    }

    private val downloadManager = context.getSystemService(Context.DOWNLOAD_SERVICE) as DownloadManager

    fun getMaxBytesOverMobile() : Long? {
        return DownloadManager.getMaxBytesOverMobile(context)
    }

    fun getRecommendedMaxBytesOverMobile() : Long? {
        return DownloadManager.getRecommendedMaxBytesOverMobile(context)
    }

    fun enqueue(request: DownloadManager.Request) : Long {
        return downloadManager.enqueue(request)
    }

    fun remove(ids: LongArray) : Int {
        return downloadManager.remove(*ids)
    }

    fun query(ids: LongArray, status: Int) : Cursor {

        val query = DownloadManager.Query()
        if (ids.isNotEmpty()) {
            query.setFilterById(*ids)
        }

        if (status != 0) {
            query.setFilterByStatus(status)
        }

        return downloadManager.query(query)
    }
}