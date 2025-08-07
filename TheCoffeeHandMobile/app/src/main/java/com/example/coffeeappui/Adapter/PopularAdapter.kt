package com.example.coffeeappui.Adapter

import android.content.Context
import android.content.Intent
import android.util.Log
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.ImageView
import androidx.recyclerview.widget.RecyclerView
import com.bumptech.glide.Glide
import com.example.coffeeappui.Activity.DetailActivity
import com.example.coffeeappui.Activity.MainActivity
import com.example.coffeeappui.Domain.ItemsModel
import com.example.coffeeappui.R
import com.example.coffeeappui.databinding.ViewholderPopularBinding
import kotlinx.coroutines.launch

class PopularAdapter(val items: MutableList<ItemsModel>) : RecyclerView.Adapter<PopularAdapter.ViewHolder>() {
    lateinit var context: Context

    class ViewHolder(val binding: ViewholderPopularBinding) : RecyclerView.ViewHolder(binding.root) {

    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): PopularAdapter.ViewHolder {
        context = parent.context
        val binding = ViewholderPopularBinding.inflate(LayoutInflater.from(parent.context), parent, false)
        return ViewHolder(binding)
    }

    override fun getItemCount(): Int = items.size

    override fun onBindViewHolder(holder: PopularAdapter.ViewHolder, position: Int) {
        val item = items[position]

        holder.binding.titleTxt.text = item.name
        holder.binding.priceTxt.text = "$${item.price}"

        // Load image từ URL hoặc sử dụng placeholder
//        Log.d("PopularAdapter", "Loading image from URL: ${item}")
        if (!item.imageUrl.isNullOrEmpty()) {
            Glide.with(context)
                .load(item.imageUrl)
                .placeholder(R.drawable.ic_launcher_background) // placeholder image
                .error(R.drawable.ic_launcher_background) // error image
                .into(holder.binding.pic)
        } else {
            // Sử dụng default image nếu không có imageUrl
            holder.binding.pic.setImageResource(R.drawable.ic_launcher_background)
        }

        holder.itemView.setOnClickListener {
            val intent = Intent(context, DetailActivity::class.java)
            intent.putExtra("object", item)
            context.startActivity(intent)
        }

//        holder.itemView.findViewById<ImageView>(R.id.imageView8).setOnClickListener {
//            val drinkId = item.id // currentItem chính là ItemsModel đang bind
//            kotlinx.coroutines.GlobalScope.launch(kotlinx.coroutines.Dispatchers.Main) {
//                (context as MainActivity).addItemToCart(drinkId, 1, context)
//            }
//        }
    }
}