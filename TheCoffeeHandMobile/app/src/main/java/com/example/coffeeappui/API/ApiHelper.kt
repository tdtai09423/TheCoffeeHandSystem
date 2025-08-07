package com.example.coffeeappui.API

import android.content.Context
import android.util.Log
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import org.json.JSONObject
import java.io.OutputStreamWriter
import java.net.HttpURLConnection
import java.net.URL

class ApiHelper(private val context: Context) {

    companion object {
        private const val API_BASE_URL = "https://10.0.2.2:7099/api/auth"
        private const val TAG = "ApiHelper"
    }

    data class LoginResponse(
        val success: Boolean,
        val token: String? = null,
        val message: String? = null
    )

    suspend fun firebaseLogin(idToken: String, fcmToken: String?): LoginResponse {
        return withContext(Dispatchers.IO) {
            try {
                val url = URL("$API_BASE_URL/firebase-login")
                val connection = url.openConnection() as HttpURLConnection

                connection.requestMethod = "POST"
                connection.setRequestProperty("Content-Type", "application/json")
                connection.doOutput = true

                // Tạo JSON payload
                val jsonPayload = JSONObject().apply {
                    put("idToken", idToken)
                    put("fmcToken", fcmToken)
                }

                // Gửi request
                val outputStream = connection.outputStream
                val writer = OutputStreamWriter(outputStream, "UTF-8")
                writer.write(jsonPayload.toString())
                writer.flush()
                writer.close()

                val responseCode = connection.responseCode
                val response = if (responseCode == HttpURLConnection.HTTP_OK) {
                    connection.inputStream.bufferedReader().use { it.readText() }
                } else {
                    connection.errorStream.bufferedReader().use { it.readText() }
                }

                if (responseCode == HttpURLConnection.HTTP_OK) {
                    val responseJson = JSONObject(response)
                    val jwtToken = responseJson.getString("token")
                    LoginResponse(success = true, token = jwtToken)
                } else {
                    val errorJson = JSONObject(response)
                    val errorMessage = errorJson.optString("message", "Login failed")
                    LoginResponse(success = false, message = errorMessage)
                }

            } catch (e: Exception) {
                Log.e(TAG, "Network error during login", e)
                LoginResponse(success = false, message = "Network error: ${e.message}")
            }
        }
    }

    suspend fun verifyToken(token: String): Boolean {
        return withContext(Dispatchers.IO) {
            try {
                val url = URL("$API_BASE_URL/verify-token")
                val connection = url.openConnection() as HttpURLConnection

                connection.requestMethod = "GET"
                connection.setRequestProperty("Authorization", "Bearer $token")

                val responseCode = connection.responseCode
                responseCode == HttpURLConnection.HTTP_OK

            } catch (e: Exception) {
                Log.e(TAG, "Error verifying token", e)
                false
            }
        }
    }

    fun saveJWTToken(token: String) {
        val sharedPref = context.getSharedPreferences("app_prefs", Context.MODE_PRIVATE)
        with(sharedPref.edit()) {
            putString("jwt_token", token)
            apply()
        }
    }

    fun getJWTToken(): String? {
        val sharedPref = context.getSharedPreferences("app_prefs", Context.MODE_PRIVATE)
        return sharedPref.getString("jwt_token", null)
    }

    fun clearJWTToken() {
        val sharedPref = context.getSharedPreferences("app_prefs", Context.MODE_PRIVATE)
        with(sharedPref.edit()) {
            remove("jwt_token")
            apply()
        }
    }
}