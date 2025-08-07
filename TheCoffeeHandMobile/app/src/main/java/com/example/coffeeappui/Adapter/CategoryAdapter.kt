package com.example.coffeeappui.Adapter

import android.content.Context
import android.content.Intent
import android.view.LayoutInflater
import android.view.ViewGroup
import androidx.core.content.ContextCompat
import androidx.recyclerview.widget.RecyclerView
import com.example.coffeeappui.Activity.ItemListActivity
import com.example.coffeeappui.Domain.CategoryModel
import com.example.coffeeappui.R
import com.example.coffeeappui.databinding.ViewholderCategoryBinding

class CategoryAdapter(val items: MutableList<CategoryModel>) :
    RecyclerView.Adapter<CategoryAdapter.ViewHolder>() {

    private lateinit var context: Context
    private var selectedPosition = -1
    private var lastSelectedPosition = -1

    inner class ViewHolder(val binding: ViewholderCategoryBinding) :
        RecyclerView.ViewHolder(binding.root)

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder {
        context = parent.context
        val binding =
            ViewholderCategoryBinding.inflate(LayoutInflater.from(parent.context), parent, false)
        return ViewHolder(binding)
    }

    override fun getItemCount(): Int = items.size

    override fun onBindViewHolder(holder: ViewHolder, position: Int) {
        val item = items[position]
        holder.binding.titleCat.text = item.title

        holder.binding.root.setOnClickListener {
            // Lấy vị trí mới nhất và an toàn nhất
            val currentPosition = holder.bindingAdapterPosition

            // Kiểm tra xem vị trí có hợp lệ không
            if (currentPosition != RecyclerView.NO_POSITION) {
                lastSelectedPosition = selectedPosition
                selectedPosition = currentPosition // Dùng vị trí hiện tại
                notifyItemChanged(lastSelectedPosition)
                notifyItemChanged(selectedPosition)

                // Các hành động khác với vị trí hiện tại
                val item = items[currentPosition]
                val intent = Intent(context, ItemListActivity::class.java).apply {
                    putExtra("id", item.id)
                    putExtra("title", item.title)
                }
                context.startActivity(intent)
            }
        }

        // Cập nhật giao diện cho item được chọn
        if (selectedPosition == position) {
            holder.binding.root.setBackgroundResource(R.drawable.dark_brown_bg)
            holder.binding.titleCat.setTextColor(context.getColor(R.color.white))
        } else {
            holder.binding.root.setBackgroundResource(R.drawable.white_bg)
            holder.binding.titleCat.setTextColor(context.getColor(R.color.darkBrown))
        }
    }
}