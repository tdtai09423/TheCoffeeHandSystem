package com.example.coffeeappui.API

import android.util.Log
import com.example.coffeeappui.Domain.CategoryModel
import com.example.coffeeappui.Domain.ItemsModel
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import okhttp3.OkHttpClient
import okhttp3.Request
import org.json.JSONObject
import java.io.IOException

class ApiRepository {

    companion object {
        private const val BASE_URL = "https://10.0.2.2:7099/api"
        private const val TAG = "ApiRepository"
    }

    private val client = getUnsafeOkHttpClient()

    suspend fun getCategories(): List<CategoryModel> {
        return withContext(Dispatchers.IO) {
            val url = "$BASE_URL/categories/paginated?pageNumber=1&pageSize=30"
            val request = Request.Builder()
                .url(url)
                .build()

            try {
                val response = client.newCall(request).execute()

                if (response.isSuccessful) { // OkHttp tự động xử lý 3xx redirect nếu cần
                    val responseBody = response.body?.string()
                    if (responseBody != null && responseBody.isNotEmpty()) {
                        Log.d(TAG, "Categories Response text: $responseBody")
                        val json = JSONObject(responseBody)
                        val items = json.getJSONArray("items")
                        List(items.length()) { i ->
                            val obj = items.getJSONObject(i)
                            CategoryModel(
                                id = obj.getString("id"),
                                title = obj.getString("name")
                            )
                        }
                    } else {
                        Log.e(TAG, "Categories: Empty response body")
                        emptyList()
                    }
                } else {
                    Log.e(TAG, "Categories: API call failed with code ${response.code}")
                    emptyList()
                }
            } catch (e: IOException) {
                Log.e(TAG, "Error fetching categories (network issue): ${e.message}", e)
                emptyList()
            } catch (e: Exception) {
                Log.e(TAG, "Error parsing categories JSON: ${e.message}", e)
                emptyList()
            }
        }
    }

    suspend fun getPopular(): List<ItemsModel> {
        return withContext(Dispatchers.IO) {
            val url = "$BASE_URL/drink/paginated/available?pageNumber=1&pageSize=6"
            val request = Request.Builder()
                .url(url)
                .build()

            try {
                val response = client.newCall(request).execute()

                if (response.isSuccessful) {
                    val responseBody = response.body?.string()
                    if (responseBody != null && responseBody.isNotEmpty()) {
                        Log.d(TAG, "Popular Response text: $responseBody")
                        val json = JSONObject(responseBody)
                        val items = json.getJSONArray("items")
                        List(items.length()) { i ->
                            val obj = items.getJSONObject(i)
                            ItemsModel(
                                id = obj.getString("id"),
                                name = obj.getString("name"),
                                description = obj.optString("description", null),
                                price = obj.getDouble("price"),
                                imageUrl = obj.optString("imageUrl", null),
                                isAvailable = if (obj.has("isAvailable")) obj.getBoolean("isAvailable") else null,
                                categoryId = if (obj.isNull("categoryId")) null else obj.getString("categoryId")
                            )
                        }
                    } else {
                        Log.e(TAG, "Popular: Empty response body")
                        emptyList()
                    }
                } else {
                    Log.e(TAG, "Popular: API call failed with code ${response.code}")
                    emptyList()
                }
            } catch (e: IOException) {
                Log.e(TAG, "Error fetching drinks (network issue): ${e.message}", e)
                emptyList()
            } catch (e: Exception) {
                Log.e(TAG, "Error parsing drinks JSON: ${e.message}", e)
                emptyList()
            }
        }
    }

    suspend fun getItemsByCategory(categoryName: String): List<ItemsModel> {
        return withContext(Dispatchers.IO) {
            // Thay đổi URL để gọi đúng API /api/drink/by-category/{categoryName}
            val url = "$BASE_URL/drink/by-category/$categoryName"
            val request = Request.Builder()
                .url(url)
                .build()

            try {
                val response = client.newCall(request).execute()
                Log.d(TAG, "Items by Category Response code: ${response.code}")
                Log.d(TAG, "Items by Category Response message: ${response.message}")

                if (response.isSuccessful) {
                    val responseBody = response.body?.string()
                    if (responseBody != null && responseBody.isNotEmpty()) {
                        Log.d(TAG, "Items by Category Response text: $responseBody")
                        // API trả về trực tiếp một mảng JSON, không có "items" wrapper
                        val jsonArray = org.json.JSONArray(responseBody)
                        List(jsonArray.length()) { i ->
                            val obj = jsonArray.getJSONObject(i)
                            ItemsModel(
                                id = obj.getString("id"),
                                name = obj.getString("name"),
                                description = obj.optString("description", null),
                                price = obj.getDouble("price"),
                                imageUrl = obj.optString("imageUrl", null),
                                isAvailable = if (obj.has("isAvailable")) obj.getBoolean("isAvailable") else null,
                                categoryId = if (obj.isNull("categoryId")) null else obj.getString("categoryId")
                            )
                        }
                    } else {
                        Log.e(TAG, "Items by Category: Empty response body")
                        emptyList()
                    }
                } else {
                    Log.e(TAG, "Items by Category: API call failed with code ${response.code}")
                    emptyList()
                }
            } catch (e: IOException) {
                Log.e(TAG, "Error fetching items by category (network issue): ${e.message}", e)
                emptyList()
            } catch (e: Exception) {
                Log.e(TAG, "Error parsing items by category JSON: ${e.message}", e)
                emptyList()
            }
        }
    }

    fun getUnsafeOkHttpClient(): OkHttpClient {
        try {
            // Create a trust manager that does not validate certificate chains
            val trustAllCerts = arrayOf<javax.net.ssl.TrustManager>(
                object : javax.net.ssl.X509TrustManager {
                    override fun checkClientTrusted(chain: Array<java.security.cert.X509Certificate>, authType: String) {}
                    override fun checkServerTrusted(chain: Array<java.security.cert.X509Certificate>, authType: String) {}
                    override fun getAcceptedIssuers(): Array<java.security.cert.X509Certificate> = arrayOf()
                }
            )

            // Install the all-trusting trust manager
            val sslContext = javax.net.ssl.SSLContext.getInstance("SSL")
            sslContext.init(null, trustAllCerts, java.security.SecureRandom())
            val sslSocketFactory = sslContext.socketFactory

            val builder = OkHttpClient.Builder()
            builder.sslSocketFactory(sslSocketFactory, trustAllCerts[0] as javax.net.ssl.X509TrustManager)
            builder.hostnameVerifier { _, _ -> true }

            return builder.build()
        } catch (e: Exception) {
            throw RuntimeException(e)
        }
    }

}