package com.example.coffeeappui.API

import android.util.Log
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import okhttp3.MediaType.Companion.toMediaTypeOrNull
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.RequestBody
import org.json.JSONArray
import org.json.JSONObject
import java.io.IOException

class ApiOrder(private val token: String) {

    companion object {
        private const val BASE_URL = "https://10.0.2.2:7099/api"
        private const val TAG = "ApiOrder"
    }

    private val client = ApiRepository().getUnsafeOkHttpClient()

    /**
     * Lấy giỏ hàng hiện tại hoặc tự động tạo mới nếu chưa có.
     */
    suspend fun getCart(): JSONObject? {
        Log.e("LOLOLOLOLOLOL", token)
        return withContext(Dispatchers.IO) {
            val url = "$BASE_URL/orders/cart"
            val request = Request.Builder()
                .url(url)
                .get()
                .addHeader("Authorization", "Bearer $token")
                .build()

            try {
                val response = client.newCall(request).execute()
                if (response.isSuccessful) {
                    val body = response.body?.string()
                    Log.d(TAG, "getCart response: $body")
                    body?.let { JSONObject(it) }
                } else {
                    Log.e(TAG, "getCart failed: ${response.code}")
                    null
                }
            } catch (e: Exception) {
                Log.e(TAG, "getCart error: ${e.message}", e)
                null
            }
        }
    }

    /**
     * Tạo mới đơn hàng thủ công.
     */
    suspend fun createOrder(userId: String): JSONObject? {
        return withContext(Dispatchers.IO) {
            val url = "$BASE_URL/orders"
            val json = JSONObject()
            json.put("userId", userId)
            json.put("status", 0) // Status = Cart
            val body = RequestBody.create("application/json".toMediaTypeOrNull(), json.toString())

            val request = Request.Builder()
                .url(url)
                .post(body)
                .addHeader("Authorization", "Bearer $token")
                .build()

            try {
                val response = client.newCall(request).execute()
                if (response.isSuccessful) {
                    val responseBody = response.body?.string()
                    Log.d(TAG, "createOrder response: $responseBody")
                    responseBody?.let { JSONObject(it) }
                } else {
                    Log.e(TAG, "createOrder failed: ${response.code}")
                    null
                }
            } catch (e: Exception) {
                Log.e(TAG, "createOrder error: ${e.message}", e)
                null
            }
        }
    }

    /**
     * Thêm sản phẩm vào giỏ hàng (order detail).
     */
    suspend fun addItemToCart(orderId: String, drinkId: String, quantity: Int): JSONObject? {
        return withContext(Dispatchers.IO) {
            val url = "$BASE_URL/order-details/cart"
            val json = JSONObject()
            json.put("orderId", orderId)
            json.put("drinkId", drinkId)
            json.put("total", quantity)
            val body = RequestBody.create("application/json".toMediaTypeOrNull(), json.toString())

            val request = Request.Builder()
                .url(url)
                .post(body)
                .addHeader("Authorization", "Bearer $token")
                .build()

            try {
                val response = client.newCall(request).execute()
                if (response.isSuccessful) {
                    val responseBody = response.body?.string()
                    Log.d(TAG, "addItemToCart response: $responseBody")
                    responseBody?.let { JSONObject(it) }
                } else {
                    Log.e(TAG, "addItemToCart failed: ${response.code}")
                    null
                }
            } catch (e: Exception) {
                Log.e(TAG, "addItemToCart error: ${e.message}", e)
                null
            }
        }
    }

    /**
     * Xác nhận đặt hàng.
     */
    suspend fun confirmOrder(orderId: String): Boolean {
        return withContext(Dispatchers.IO) {
            val url = "$BASE_URL/orders/$orderId/confirm"
            val request = Request.Builder()
                .url(url)
                .post(RequestBody.create(null, ByteArray(0))) // Empty body
                .addHeader("Authorization", "Bearer $token")
                .build()

            try {
                val response = client.newCall(request).execute()
                if (response.isSuccessful) {
                    Log.d(TAG, "confirmOrder success")
                    true
                } else {
                    Log.e(TAG, "confirmOrder failed: ${response.code}")
                    false
                }
            } catch (e: Exception) {
                Log.e(TAG, "confirmOrder error: ${e.message}", e)
                false
            }
        }
    }

    /**
     * Hoàn tất đơn hàng.
     */
    suspend fun completeOrder(orderId: String): Boolean {
        return withContext(Dispatchers.IO) {
            val url = "$BASE_URL/orders/$orderId/complete"
            val request = Request.Builder()
                .url(url)
                .post(RequestBody.create(null, ByteArray(0)))
                .addHeader("Authorization", "Bearer $token")
                .build()

            try {
                val response = client.newCall(request).execute()
                if (response.isSuccessful) {
                    Log.d(TAG, "completeOrder success")
                    true
                } else {
                    Log.e(TAG, "completeOrder failed: ${response.code}")
                    false
                }
            } catch (e: Exception) {
                Log.e(TAG, "completeOrder error: ${e.message}", e)
                false
            }
        }
    }

    /**
     * Huỷ đơn hàng.
     */
    suspend fun cancelOrder(orderId: String): Boolean {
        return withContext(Dispatchers.IO) {
            val url = "$BASE_URL/orders/$orderId/cancel"
            val request = Request.Builder()
                .url(url)
                .post(RequestBody.create(null, ByteArray(0)))
                .addHeader("Authorization", "Bearer $token")
                .build()

            try {
                val response = client.newCall(request).execute()
                if (response.isSuccessful) {
                    Log.d(TAG, "cancelOrder success")
                    true
                } else {
                    Log.e(TAG, "cancelOrder failed: ${response.code}")
                    false
                }
            } catch (e: Exception) {
                Log.e(TAG, "cancelOrder error: ${e.message}", e)
                false
            }
        }
    }

    /**
     * Xoá đơn hàng.
     */
    suspend fun deleteOrder(orderId: String): Boolean {
        return withContext(Dispatchers.IO) {
            val url = "$BASE_URL/orders/$orderId"
            val request = Request.Builder()
                .url(url)
                .delete()
                .addHeader("Authorization", "Bearer $token")
                .build()

            try {
                val response = client.newCall(request).execute()
                if (response.isSuccessful) {
                    Log.d(TAG, "deleteOrder success")
                    true
                } else {
                    Log.e(TAG, "deleteOrder failed: ${response.code}")
                    false
                }
            } catch (e: Exception) {
                Log.e(TAG, "deleteOrder error: ${e.message}", e)
                false
            }
        }
    }

    /**
     * Cập nhật số lượng sản phẩm trong giỏ hàng.
     */
    suspend fun updateItemQuantity(orderDetailId: String, quantity: Int): Boolean {
        return withContext(Dispatchers.IO) {
            val url = "$BASE_URL/order-details/$orderDetailId"
            val json = JSONObject()
            json.put("total", quantity)

            val body = RequestBody.create("application/json".toMediaTypeOrNull(), json.toString())

            val request = Request.Builder()
                .url(url)
                .put(body)
                .addHeader("Authorization", "Bearer $token")
                .build()

            try {
                val response = client.newCall(request).execute()
                if (response.isSuccessful) {
                    Log.d(TAG, "updateItemQuantity success")
                    true
                } else {
                    Log.e(TAG, "updateItemQuantity failed: ${response.code}")
                    false
                }
            } catch (e: Exception) {
                Log.e(TAG, "updateItemQuantity error: ${e.message}", e)
                false
            }
        }
    }
    /**
     * Xoá sản phẩm khỏi giỏ hàng.
     */
    suspend fun removeItemFromCart(orderDetailId: String): Boolean {
        return withContext(Dispatchers.IO) {
            val url = "$BASE_URL/order-details/cart/$orderDetailId"
            val body = RequestBody.create("application/json".toMediaTypeOrNull(), "{}")

            val request = Request.Builder()
                .url(url)
                .put(body)
                .addHeader("Authorization", "Bearer $token")
                .build()

            try {
                val response = client.newCall(request).execute()
                if (response.isSuccessful) {
                    Log.d(TAG, "removeItemFromCart success")
                    true
                } else {
                    Log.e(TAG, "removeItemFromCart failed: ${response.code}")
                    false
                }
            } catch (e: Exception) {
                Log.e(TAG, "removeItemFromCart error: ${e.message}", e)
                false
            }
        }
    }

}
