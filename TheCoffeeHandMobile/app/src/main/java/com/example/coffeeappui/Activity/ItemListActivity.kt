package com.example.coffeeappui.Activity

import android.os.Bundle
import android.view.View
import androidx.activity.enableEdgeToEdge
import androidx.appcompat.app.AppCompatActivity
import androidx.core.view.ViewCompat
import androidx.core.view.WindowInsetsCompat
import androidx.lifecycle.Observer
import androidx.recyclerview.widget.LinearLayoutManager
import com.example.coffeeappui.Adapter.ItemListCategoryAdapter
import com.example.coffeeappui.R
import com.example.coffeeappui.ViewModel.MainViewModel
import com.example.coffeeappui.databinding.ActivityItemListBinding

class ItemListActivity : AppCompatActivity() {
    lateinit var binding: ActivityItemListBinding
    private val viewModel= MainViewModel()
    private var id:String="" // id ở đây thực chất là tên danh mục (categoryName)
    private var title:String="" // title cũng là tên danh mục

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        binding=ActivityItemListBinding.inflate(layoutInflater)
        setContentView(binding.root)

        getBundle()
        initList()
    }

    private fun initList() {
        binding.apply {
            progressBar.visibility= View.VISIBLE
            // Gọi hàm loadItems với tên danh mục (title)
            viewModel.loadItems(title) // Sử dụng 'title' vì nó là tên danh mục được truyền vào
            viewModel.categoryItems.observe(this@ItemListActivity, Observer { // Quan sát LiveData mới
                listView.layoutManager=
                    LinearLayoutManager(
                        this@ItemListActivity, LinearLayoutManager.VERTICAL, false
                    )
                listView.adapter= ItemListCategoryAdapter(it.toMutableList())
                progressBar.visibility = View.GONE
            })
            backBtn.setOnClickListener {
                finish()
            }
        }
    }

    private fun getBundle(){
        id=intent.getStringExtra("id")!! // id ở đây được dùng như category name
        title=intent.getStringExtra("title")!! // title cũng là category name

        binding.categoryTxt.text=title
    }
}