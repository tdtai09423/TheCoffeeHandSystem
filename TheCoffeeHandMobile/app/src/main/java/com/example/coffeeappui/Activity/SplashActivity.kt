package com.example.coffeeappui.Activity

import android.content.Intent
import android.os.Bundle
import android.util.Log
import android.widget.Toast
import androidx.activity.enableEdgeToEdge
import androidx.activity.result.contract.ActivityResultContracts
import androidx.appcompat.app.AppCompatActivity
import com.example.coffeeappui.R
import com.example.coffeeappui.databinding.ActivitySplashBinding
import com.google.android.gms.auth.api.signin.GoogleSignIn
import com.google.android.gms.auth.api.signin.GoogleSignInAccount
import com.google.android.gms.auth.api.signin.GoogleSignInClient
import com.google.android.gms.auth.api.signin.GoogleSignInOptions
import com.google.android.gms.common.api.ApiException
import com.google.firebase.auth.FirebaseAuth
import com.google.firebase.auth.GoogleAuthProvider
import com.google.firebase.messaging.FirebaseMessaging
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import org.json.JSONObject
import java.io.OutputStreamWriter
import java.net.HttpURLConnection
import java.net.URL
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.RequestBody

class SplashActivity : AppCompatActivity() {

    private lateinit var binding: ActivitySplashBinding
    private lateinit var googleSignInClient: GoogleSignInClient
    private lateinit var firebaseAuth: FirebaseAuth

    // API endpoint - thay đổi theo server của bạn
    private val API_BASE_URL = "https://10.0.2.2:7099/api/auth"

    private val googleSignInLauncher = registerForActivityResult(
        ActivityResultContracts.StartActivityForResult()
    ) { result ->
        if (result.resultCode == RESULT_OK) {
            val task = GoogleSignIn.getSignedInAccountFromIntent(result.data)
            try {
                val account = task.getResult(ApiException::class.java)
                firebaseAuthWithGoogle(account)
            } catch (e: ApiException) {
                Log.e("GoogleSignIn", "Google sign in failed", e)
                Toast.makeText(this, "Google sign in failed: ${e.message}", Toast.LENGTH_SHORT).show()
            }
        }
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        binding = ActivitySplashBinding.inflate(layoutInflater)
        setContentView(binding.root)

        // Khởi tạo Firebase Auth
        firebaseAuth = FirebaseAuth.getInstance()

        // Cấu hình Google Sign-In
        val gso = GoogleSignInOptions.Builder(GoogleSignInOptions.DEFAULT_SIGN_IN)
            .requestIdToken(getString(R.string.default_web_client_id)) // Từ google-services.json
            .requestEmail()
            .build()

        googleSignInClient = GoogleSignIn.getClient(this, gso)

        // Kiểm tra xem user đã đăng nhập chưa
        val currentUser = firebaseAuth.currentUser
        if (currentUser != null) {
            // User đã đăng nhập, chuyển đến MainActivity
            navigateToMain()
            return
        }

        // Thiết lập click listeners
        binding.startBtn.setOnClickListener {
            signInWithGoogle()
        }

        // Nếu có nút đăng nhập thường
        // binding.emailLoginBtn?.setOnClickListener {
        //     // Chuyển đến EmailLoginActivity
        // }
    }

    private fun signInWithGoogle() {
        val signInIntent = googleSignInClient.signInIntent
        googleSignInLauncher.launch(signInIntent)
    }

    private fun firebaseAuthWithGoogle(account: GoogleSignInAccount) {
        Log.d("GoogleSignIn", "firebaseAuthWithGoogle: ${account.id}")

        val credential = GoogleAuthProvider.getCredential(account.idToken, null)
        firebaseAuth.signInWithCredential(credential)
            .addOnCompleteListener(this) { task ->
                if (task.isSuccessful) {
                    Log.d("GoogleSignIn", "signInWithCredential:success")
                    val user = firebaseAuth.currentUser
                    user?.getIdToken(true)?.addOnCompleteListener { tokenTask ->
                        if (tokenTask.isSuccessful) {
                            val idToken = tokenTask.result?.token
                            if (idToken != null) {
                                // Gọi API để đăng nhập
                                authenticateWithBackend(idToken)
                            }
                        } else {
                            Log.e("GoogleSignIn", "Failed to get ID token", tokenTask.exception)
                            Toast.makeText(this, "Failed to get authentication token", Toast.LENGTH_SHORT).show()
                        }
                    }
                } else {
                    Log.w("GoogleSignIn", "signInWithCredential:failure", task.exception)
                    Toast.makeText(this, "Authentication failed: ${task.exception?.message}", Toast.LENGTH_SHORT).show()
                }
            }
    }

    private fun authenticateWithBackend(idToken: String) {
        // Lấy FCM token
        FirebaseMessaging.getInstance().token.addOnCompleteListener { task ->
            if (!task.isSuccessful) {
                Log.w("FCM", "Fetching FCM registration token failed", task.exception)
                // Tiếp tục với FCM token null
                callFirebaseLoginAPI(idToken, null)
                return@addOnCompleteListener
            }

            val fcmToken = task.result
            callFirebaseLoginAPI(idToken, fcmToken)
        }
    }

    private fun callFirebaseLoginAPI(idToken: String, fcmToken: String?) {
        CoroutineScope(Dispatchers.IO).launch {
            try {
                // Lấy OkHttpClient bỏ SSL check
                val client = com.example.coffeeappui.API.ApiRepository().getUnsafeOkHttpClient()

                // Tạo JSON payload
                val jsonPayload = JSONObject().apply {
                    put("idToken", idToken)
                    put("fmcToken", fcmToken)
                }

                val mediaType = "application/json; charset=utf-8".toMediaType()
                val body = RequestBody.create(mediaType, jsonPayload.toString())

                val request = Request.Builder()
                    .url("$API_BASE_URL/firebase-login")
                    .post(body)
                    .build()

                val response = client.newCall(request).execute()
                val responseBody = response.body?.string()

                withContext(Dispatchers.Main) {
                    if (response.isSuccessful && responseBody != null) {
                        val responseJson = JSONObject(responseBody)
                        val jwtToken = responseJson.getString("token")
                        saveJWTToken(jwtToken)
                        navigateToMain()
                    } else {
                        val errorMsg = responseBody ?: "Unknown error"
                        Log.e("API", "Login failed: $errorMsg")
                        Toast.makeText(this@SplashActivity, "Login failed: $errorMsg", Toast.LENGTH_SHORT).show()
                    }
                }
            } catch (e: Exception) {
                Log.e("API", "Network error", e)
                withContext(Dispatchers.Main) {
                    Toast.makeText(this@SplashActivity, "Network error: ${e.message}", Toast.LENGTH_SHORT).show()
                }
            }
        }
    }


    private fun saveJWTToken(token: String) {
        val sharedPref = getSharedPreferences("app_prefs", MODE_PRIVATE)
        with(sharedPref.edit()) {
            putString("jwt_token", token)
            apply()
        }
    }

    private fun navigateToMain() {
        startActivity(Intent(this, MainActivity::class.java))
        finish()
    }

    override fun onStart() {
        super.onStart()
        // Kiểm tra xem user đã đăng nhập chưa
        val currentUser = firebaseAuth.currentUser
        if (currentUser != null) {
            navigateToMain()
        }
    }
}