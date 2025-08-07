package com.example.coffeeappui.Adapter

import android.content.Context
import android.util.Log
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import androidx.recyclerview.widget.RecyclerView
import com.bumptech.glide.Glide
import com.bumptech.glide.load.resource.bitmap.CenterCrop
import com.bumptech.glide.request.RequestOptions
import com.example.coffeeappui.API.ApiOrder
import com.example.coffeeappui.Domain.CartItem
import com.example.coffeeappui.Domain.ItemsModel
import com.example.coffeeappui.Helper.ChangeNumberItemsListener
import com.example.coffeeappui.Helper.ManagmentCart
import com.example.coffeeappui.databinding.ViewholderCartBinding
import com.google.firebase.auth.FirebaseAuth
import kotlinx.coroutines.launch

class CartAdapter(private val listItemSelected: ArrayList<CartItem>,
                  context: Context,
                  private val token: String,
                  private val orderId: String,
                  private val reloadCart: () -> Unit,
                  var changeNumberItemsListener: ChangeNumberItemsListener? = null
) : RecyclerView.Adapter<CartAdapter.Viewholder>() {
    class Viewholder(val binding: ViewholderCartBinding) : RecyclerView.ViewHolder(binding.root)

    private val managementCart = ManagmentCart(context)

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): CartAdapter.Viewholder {
        val binding =
            ViewholderCartBinding.inflate(LayoutInflater.from(parent.context), parent, false)

        return Viewholder(binding)
    }

    override fun onBindViewHolder(holder: CartAdapter.Viewholder, position: Int) {
        val item = listItemSelected[position]

        holder.binding.titleTxt.text = item.name
        holder.binding.feeEachItem.text = "$${"%.2f".format(item.price)}"
        holder.binding.totalEachItem.text = "$${"%.2f".format(item.price * item.quantity)}"
        holder.binding.numberItemTxt.text = item.quantity.toString()
        Log.d("CartAdapter", "Drink: ${item}")

        Glide.with(holder.itemView.context)
            .load(item.imageUrl)
            .into(holder.binding.picCart)

        holder.binding.plusEachItem.setOnClickListener {
            kotlinx.coroutines.GlobalScope.launch(kotlinx.coroutines.Dispatchers.Main) {
                val apiOrder = ApiOrder(token)
                val response = apiOrder.addItemToCart(orderId = orderId, drinkId = item.id, quantity = 1)
                if (response != null) reloadCart()
            }
        }

        holder.binding.minusEachItem.setOnClickListener {
            kotlinx.coroutines.GlobalScope.launch(kotlinx.coroutines.Dispatchers.Main) {
                val apiOrder = ApiOrder(token)
                val newQuantity = item.quantity - 1
                val success = if (newQuantity <= 0) {
                    apiOrder.removeItemFromCart(item.orderDetailId)
                } else {
                    apiOrder.updateItemQuantity(item.orderDetailId, newQuantity)
                }
                if (success) reloadCart()
            }
        }

        holder.binding.removeItemBtn.setOnClickListener {
            kotlinx.coroutines.GlobalScope.launch(kotlinx.coroutines.Dispatchers.Main) {
                val apiOrder = ApiOrder(token)
                val success = apiOrder.removeItemFromCart(item.orderDetailId)
                if (success) reloadCart()
            }
        }


    }

    override fun getItemCount(): Int = listItemSelected.size

}