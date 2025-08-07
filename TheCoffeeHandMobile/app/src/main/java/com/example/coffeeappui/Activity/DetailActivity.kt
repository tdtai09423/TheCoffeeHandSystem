package com.example.coffeeappui.Activity

import android.content.Context
import android.content.Intent
import android.os.Bundle
import android.util.Log
import androidx.activity.enableEdgeToEdge
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.ViewCompat
import androidx.core.view.WindowInsetsCompat
import com.bumptech.glide.Glide
import com.example.coffeeappui.API.ApiOrder
import com.example.coffeeappui.Domain.ItemsModel
import com.example.coffeeappui.Helper.ManagmentCart
import com.example.coffeeappui.R
import com.example.coffeeappui.Utils.NotificationHelper
import com.example.coffeeappui.databinding.ActivityDetailBinding
import com.google.firebase.auth.FirebaseAuth
import kotlinx.coroutines.launch

class DetailActivity : AppCompatActivity() {
    lateinit var binding: ActivityDetailBinding
    private lateinit var item: ItemsModel
    private lateinit var managmentCart: ManagmentCart

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        binding=ActivityDetailBinding.inflate(layoutInflater)
        setContentView(binding.root)

        managmentCart=ManagmentCart(this)

        bundle()
        initListSize()

    }

    private fun initListSize() {
        binding.apply {
            smallBtn.setOnClickListener {
                smallBtn.setBackgroundResource(R.drawable.stroke_brown_bg)
                mediumBtn.setBackgroundResource(0)
                largeBtn.setBackgroundResource(0)
            }
            mediumBtn.setOnClickListener {
                smallBtn.setBackgroundResource(0)
                mediumBtn.setBackgroundResource(R.drawable.stroke_brown_bg)
                largeBtn.setBackgroundResource(0)
            }
            largeBtn.setOnClickListener {
                smallBtn.setBackgroundResource(0)
                mediumBtn.setBackgroundResource(0)
                largeBtn.setBackgroundResource(R.drawable.stroke_brown_bg)
            }
        }
    }

    private fun bundle(){
        binding.apply {
            item = intent.getParcelableExtra<ItemsModel>("object") ?: run {
                Log.e("DetailActivity", "Intent không chứa dữ liệu ItemsModel!")
                finish()
                return
            }

            Glide.with(this@DetailActivity)
                .load(item.imageUrl)
                .into(binding.picMain)

            titleTxt.text = item.name
            descriptionTxt.text = item.description
            priceTxt.text = "$" + item.price
            ratingTxt.text = "4"

            addToCartBtn.setOnClickListener {
                val quantity = numberitemTxt.text.toString().toInt()
                kotlinx.coroutines.GlobalScope.launch(kotlinx.coroutines.Dispatchers.Main) {
                    val token = getFirebaseToken()
                    if (token != null) {
                        addItemToCartWithFetchCart(item.id, quantity, token, this@DetailActivity)
                        finish()
                    } else {
                        Log.e("DetailActivity", "Chưa đăng nhập")
                    }
                }
            }

            backBtn.setOnClickListener {
                finish()
            }

            plusCart.setOnClickListener {
                numberitemTxt.text = (Integer.parseInt(numberitemTxt.text.toString()) + 1).toString()
            }

            minusBtn.setOnClickListener {
//                if (item.numberInCart > 0) {
//                    numberitemTxt.text = (Integer.parseInt(item.numberInCart.toString()) - 1).toString()
//                }
            }
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
}