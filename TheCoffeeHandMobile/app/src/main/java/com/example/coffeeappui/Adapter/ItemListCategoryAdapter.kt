package com.example.coffeeappui.Adapter

import android.content.Context
import android.content.Intent
import android.util.Log
import android.view.LayoutInflater
import android.view.ViewGroup
import androidx.recyclerview.widget.RecyclerView
import com.bumptech.glide.Glide
import com.example.coffeeappui.Activity.DetailActivity
import com.example.coffeeappui.Domain.ItemsModel // Đảm bảo import đúng ItemsModel
import com.example.coffeeappui.R // Import R cho placeholder image
import com.example.coffeeappui.databinding.ViewholderItemPicLeftBinding
import com.example.coffeeappui.databinding.ViewholderItemPicRightBinding

class ItemListCategoryAdapter(val items: MutableList<ItemsModel>) //
    : RecyclerView.Adapter<RecyclerView.ViewHolder>() { //

    companion object{ //
        const val TYPE_ITEM1 = 0 //
        const val TYPE_ITEM2 = 1 //
    }

    lateinit var context: Context //
    override fun getItemViewType(position: Int): Int { //
        return if(position%2==0) TYPE_ITEM1 else TYPE_ITEM2 //
    }

    class ViewHolderItem1(val binding: ViewholderItemPicRightBinding): //
        RecyclerView.ViewHolder(binding.root) //

    class ViewHolderItem2(val binding: ViewholderItemPicLeftBinding): //
        RecyclerView.ViewHolder(binding.root) //

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): RecyclerView.ViewHolder { //
        context=parent.context //
        return when (viewType){ //
            TYPE_ITEM1 -> { //
                val binding = ViewholderItemPicRightBinding.inflate( //
                    LayoutInflater.from(context), parent, false) //
                ViewHolderItem1(binding) //
            }
            TYPE_ITEM2 -> { //
                val binding = ViewholderItemPicLeftBinding.inflate( //
                    LayoutInflater.from(context), parent, false) //
                ViewHolderItem2(binding) //
            }
            else -> throw IllegalArgumentException("Invalid view type")
        }
    }

    override fun getItemCount(): Int = items.size //

    override fun onBindViewHolder(holder: RecyclerView.ViewHolder, position: Int) { //
        val item = items[position] // Lấy đối tượng ItemsModel từ danh sách

        // Dùng các thuộc tính của ItemsModel để bind dữ liệu
        val titleTxt = item.name //
        val priceTxt = "$${item.price}" //
        // Rating không có trong ItemsModel, bạn có thể thêm vào hoặc bỏ qua
        val rating = 0f // Giả sử rating mặc định là 0 hoặc lấy từ model nếu có
        val picUrl = item.imageUrl //

        // Binding dữ liệu dựa trên loại ViewHolder
        when (holder) { //
            is ViewHolderItem1 -> { //
                holder.binding.titleTxt.text = titleTxt //
                holder.binding.priceTxt.text = priceTxt //
                holder.binding.ratingBar.rating = rating //

                Log.d("LOLOLOLLOO", "Loading image from URL: $item.imageUrl")
                if (!picUrl.isNullOrEmpty()) { //
                    Glide.with(context) //
                        .load(picUrl) //
                        .placeholder(R.drawable.ic_launcher_background) // Placeholder image
                        .error(R.drawable.ic_launcher_background) // Error image
                        .into(holder.binding.picMain) //
                } else {
                    holder.binding.picMain.setImageResource(R.drawable.ic_launcher_background)
                }

                holder.itemView.setOnClickListener { //
                    val intent = Intent(context, DetailActivity::class.java) //
                    intent.putExtra("object", item) // Truyền đối tượng ItemsModel
                    context.startActivity(intent) //
                }
            }

            is ViewHolderItem2 -> { //
                holder.binding.titleTxt.text = titleTxt //
                holder.binding.priceTxt.text = priceTxt //
                holder.binding.ratingBar.rating = rating //

                if (!picUrl.isNullOrEmpty()) { //
                    Glide.with(context) //
                        .load(picUrl) //
                        .placeholder(R.drawable.ic_launcher_background) // Placeholder image
                        .error(R.drawable.ic_launcher_background) // Error image
                        .into(holder.binding.picMain) //
                } else {
                    holder.binding.picMain.setImageResource(R.drawable.ic_launcher_background)
                }

                holder.itemView.setOnClickListener { //
                    val intent = Intent(context, DetailActivity::class.java) //
                    intent.putExtra("object", item) // Truyền đối tượng ItemsModel
                    context.startActivity(intent) //
                }
            }
        }
    }
}