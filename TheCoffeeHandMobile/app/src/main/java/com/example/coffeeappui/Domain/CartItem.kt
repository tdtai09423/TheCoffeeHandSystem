package com.example.coffeeappui.Domain

data class CartItem(
    val id: String,
    val name: String,
    val price: Double,
    val imageUrl: String?,
    val quantity: Int,
    val orderDetailId: String = "",
    val orderId: String = ""
)

