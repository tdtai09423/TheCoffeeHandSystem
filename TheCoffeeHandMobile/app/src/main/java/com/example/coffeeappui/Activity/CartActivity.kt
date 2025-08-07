package com.example.coffeeappui.Activity

import android.os.Bundle
import android.util.Log
import androidx.activity.enableEdgeToEdge
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.ViewCompat
import androidx.core.view.WindowInsetsCompat
import androidx.recyclerview.widget.LinearLayoutManager
import com.example.coffeeappui.API.ApiOrder
import com.example.coffeeappui.Adapter.CartAdapter
import com.example.coffeeappui.Domain.CartItem
import com.example.coffeeappui.Helper.ChangeNumberItemsListener
import com.example.coffeeappui.Helper.ManagmentCart
import com.example.coffeeappui.R
import com.example.coffeeappui.databinding.ActivityCartBinding
import com.google.firebase.auth.FirebaseAuth
import kotlinx.coroutines.launch

class CartActivity : AppCompatActivity() {
    lateinit var binding: ActivityCartBinding
    lateinit var managmentCart: ManagmentCart
    private var tax: Double = 0.0

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        binding= ActivityCartBinding.inflate(layoutInflater)
        setContentView(binding.root)

        managmentCart = ManagmentCart(this)

//        caculateCart()
        setVariable()
        loadCart()
    }

    private fun loadCart() {
        kotlinx.coroutines.GlobalScope.launch(kotlinx.coroutines.Dispatchers.Main) {
            val token = getFirebaseToken()
            if (token == null) {
                Log.e("CartActivity", "Chưa đăng nhập")
                return@launch
            }

            val apiOrder = ApiOrder(token)
            val cart = apiOrder.getCart()
            if (cart == null) {
                Log.e("CartActivity", "Không load được giỏ hàng")
                return@launch
            }

            val orderDetails = cart.optJSONArray("orderDetails")
            if (orderDetails == null || orderDetails.length() == 0) {
                Log.d("CartActivity", "Giỏ hàng trống")
                return@launch
            }

            val cartItems = ArrayList<CartItem>()
            var totalPrice = 0.0

            for (i in 0 until orderDetails.length()) {
                val itemObj = orderDetails.getJSONObject(i)
                val drink = itemObj.getJSONObject("drink")
                Log.d("CartActivity", "Drink: $itemObj")

                val item = CartItem(
                    id = itemObj.optString("drinkId", "0"),
                    name = drink.getString("name"),
                    price = drink.getDouble("price"),
                    imageUrl = drink.optString("imageUrl", null),
                    quantity = itemObj.optInt("total", 1),
                    orderDetailId = itemObj.optString("id", "0"),
                    orderId = itemObj.optString("orderId", "0")
                )
                totalPrice += item.price * item.quantity
                cartItems.add(item)
            }
            Log.d("ADDDDDD", "addItemToCart body: ${cart.getString("id")}")
            // Gắn adapter
            binding.cartView.layoutManager = LinearLayoutManager(this@CartActivity)
            binding.cartView.adapter = CartAdapter(cartItems, context = this@CartActivity, token, cart.getString("id"), reloadCart = {
                loadCart() // callback để reload
            })

            // Tính toán tổng tiền
            val tax = totalPrice * 0.02
            val delivery = 1.0
            val total = totalPrice + tax + delivery

            binding.totalFeeTxt.text = "$${"%.2f".format(totalPrice)}"
            binding.taxTxt.text = "$${"%.2f".format(tax)}"
            binding.deliverTxt.text = "$${"%.2f".format(delivery)}"
            binding.totalTxt.text = "$${"%.2f".format(total)}"
        }
    }


    private fun setVariable() {
        binding.backBtn.setOnClickListener {
            finish()
        }
    }

//    private fun caculateCart() {
//        val percentTax = 0.02
//        val delivery = 15
//        tax = Math.round(managmentCart.getTotalFee() * percentTax * 100) / 100.0
//        val total = Math.round(managmentCart.getTotalFee() + tax + delivery) * 100 / 100
//        val itemTotal = Math.round(managmentCart.getTotalFee() * 100) / 100
//        binding.apply {
//            totalFeeTxt.text = "$$itemTotal"
//            taxTxt.text = "$$tax"
//            deliverTxt.text = "$$delivery"
//            totalTxt.text = "$$total"
//        }
//
//    }

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
}