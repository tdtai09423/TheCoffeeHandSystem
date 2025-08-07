package com.example.coffeeappui.Activity

import android.content.Context
import android.content.Intent
import android.os.Bundle
import android.util.Log
import android.view.View
import androidx.activity.enableEdgeToEdge
import androidx.appcompat.app.AppCompatActivity
import androidx.recyclerview.widget.GridLayoutManager
import androidx.recyclerview.widget.LinearLayoutManager
import com.bumptech.glide.Glide
import com.example.coffeeappui.API.ApiOrder
import com.example.coffeeappui.Adapter.CategoryAdapter
import com.example.coffeeappui.Adapter.PopularAdapter
import com.example.coffeeappui.R
import com.example.coffeeappui.Utils.NotificationHelper
import com.example.coffeeappui.ViewModel.MainViewModel
import com.example.coffeeappui.databinding.ActivityMainBinding
import com.google.firebase.auth.FirebaseAuth
import com.google.android.gms.auth.api.signin.GoogleSignIn
import com.google.android.gms.auth.api.signin.GoogleSignInOptions
import kotlinx.coroutines.launch


class MainActivity : AppCompatActivity() {
    lateinit var binding: ActivityMainBinding
    private val viewModel=MainViewModel()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        com.google.firebase.FirebaseApp.initializeApp(this)

        enableEdgeToEdge()
        binding = ActivityMainBinding.inflate(layoutInflater)
        setContentView(binding.root)

        // Gọi API kiểm tra giỏ hàng
        checkCart()

        if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.TIRAMISU) {
            if (checkSelfPermission(android.Manifest.permission.POST_NOTIFICATIONS) != android.content.pm.PackageManager.PERMISSION_GRANTED) {
                requestPermissions(arrayOf(android.Manifest.permission.POST_NOTIFICATIONS), 1001)
            }
        }

        binding.logOutBtn.setOnClickListener {
            // Đăng xuất Firebase
            FirebaseAuth.getInstance().signOut()

            // Đăng xuất GoogleSignIn
            val gso = GoogleSignInOptions.Builder(GoogleSignInOptions.DEFAULT_SIGN_IN)
                .requestIdToken(getString(R.string.default_web_client_id))
                .requestEmail()
                .build()
            val googleSignInClient = GoogleSignIn.getClient(this, gso)
            googleSignInClient.signOut().addOnCompleteListener {
                // Sau khi signOut Google thành công, điều hướng về Splash
                val intent = Intent(this, SplashActivity::class.java)
                intent.flags = Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_CLEAR_TASK
                startActivity(intent)
                finish()
            }
        }



        initBanner()
        initCategory()
        initPopular()
        initBottomMenu()
    }

    override fun onRequestPermissionsResult(requestCode: Int, permissions: Array<out String>, grantResults: IntArray) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults)
        if (requestCode == 1001) {
            if (grantResults.isNotEmpty() && grantResults[0] == android.content.pm.PackageManager.PERMISSION_GRANTED) {
                Log.d("MainActivity", "Đã được cấp quyền POST_NOTIFICATIONS")
            } else {
                Log.w("MainActivity", "Người dùng từ chối quyền thông báo")
            }
        }
    }

    private fun initBottomMenu() {
        binding.cartBtn.setOnClickListener {
            startActivity(Intent(this, CartActivity::class.java))
        }
        binding.mapBtn.setOnClickListener {
            startActivity(Intent(this, MapActivity::class.java))
        }
    }

    private fun initBanner(){
        binding.progressBarBanner.visibility = View.GONE

        Glide.with(this@MainActivity)
            .load(R.drawable.banner)
            .into(binding.banner)

    }

    private fun initCategory(){
        binding.progressBarCategory.visibility = View.VISIBLE
        viewModel.loadCategory()
        viewModel.categories.observe(this) {
            binding.recyclerViewCat.layoutManager =
                LinearLayoutManager(this, LinearLayoutManager.HORIZONTAL, false)
            binding.recyclerViewCat.adapter = CategoryAdapter(it.toMutableList())
            binding.progressBarCategory.visibility = View.GONE
        }
    }

    private fun initPopular(){
        binding.progressBarPopular.visibility = View.VISIBLE
        viewModel.loadPopular()
        viewModel.popularItems.observe(this) {
            binding.recyclerViewPopular.layoutManager = GridLayoutManager(this, 2)
            binding.recyclerViewPopular.adapter = PopularAdapter(it.toMutableList())
            binding.progressBarPopular.visibility = View.GONE
        }
    }

    suspend fun getFirebaseToken(): String? {
        return kotlinx.coroutines.suspendCancellableCoroutine { cont ->
            val user = FirebaseAuth.getInstance().currentUser
            if (user != null) {
                user.getIdToken(true)
                    .addOnSuccessListener { result ->
                        val token = result.token
                        cont.resume(token, null)
                    }
                    .addOnFailureListener { e ->
                        cont.resume(null, null)
                    }
            } else {
                cont.resume(null, null)
            }
        }
    }

    private fun checkCart() {
        // Chạy trong coroutine
        kotlinx.coroutines.GlobalScope.launch(kotlinx.coroutines.Dispatchers.Main) {
            val token = getFirebaseToken()
            if (token != null) {
                val apiOrder = ApiOrder(token)
                val cart = apiOrder.getCart()
                if (cart != null) {
                    val orderDetails = cart.optJSONArray("orderDetails")
                    if (orderDetails != null && orderDetails.length() > 0) {
                        val count = orderDetails.length()
                        val message = "Có $count món trong giỏ hàng"

                        // Hiện TextView
                        binding.notiMain.text = message
                        binding.notiMain.visibility = View.VISIBLE

                        // Hiện Notification
                        val notificationHelper = NotificationHelper(this@MainActivity)
                        notificationHelper.showNotification("Giỏ hàng", message)
                    }
                }
            } else {
                Log.e("MainActivity", "User chưa đăng nhập hoặc không lấy được token")
            }
        }
    }

    suspend fun addItemToCartWithFetchCart(drinkId: String, quantity: Int, token: String, context: Context) {
        val apiOrder = ApiOrder(token)

        // Gọi API lấy giỏ hàng
        val cart = apiOrder.getCart()
        val orderId = cart?.optString("id")

        if (orderId.isNullOrEmpty()) {
            Log.e("AddToCart", "Không tìm thấy giỏ hàng.")
            return
        }

        // Gọi API thêm sản phẩm
        apiOrder.addItemToCart(orderId, drinkId, quantity)

        // Hiện thông báo
        NotificationHelper(context).showNotification(
            "Giỏ hàng",
            "Đã thêm $quantity sản phẩm vào giỏ"
        )
    }


    suspend fun addItemToCart(drinkId: String, quantity: Int, context: Context) {
        val token = getFirebaseToken()
        if (token != null) {
            val apiOrder = ApiOrder(token)
            val cart = apiOrder.getCart()
            val orderId = cart?.getString("id")
            if (orderId != null) {
                apiOrder.addItemToCart(orderId, drinkId, quantity)
                // Hiện notification hệ thống
                NotificationHelper(context).showNotification("Giỏ hàng", "Đã thêm $quantity sản phẩm vào giỏ")
            } else {
                Log.e("AddToCart", "Không tìm thấy giỏ hàng.")
            }
        } else {
            Log.e("AddToCart", "Chưa đăng nhập hoặc không lấy được token.")
        }
    }


}