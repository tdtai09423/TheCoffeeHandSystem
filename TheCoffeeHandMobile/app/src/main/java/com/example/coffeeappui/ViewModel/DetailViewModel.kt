package com.example.coffeeappui.ViewModel

import android.util.Log
import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import com.example.coffeeappui.API.ApiRepository
import com.example.coffeeappui.Domain.BannerModel
import com.example.coffeeappui.Domain.CategoryModel
import com.example.coffeeappui.Domain.ItemsModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.launch

class DetailViewModel : ViewModel(){
    private val repository = ApiRepository()

//    fun loadBanner(): LiveData<MutableList<BannerModel>> {
//        return repository.loadBanner()
//    }

    //    fun loadItems(categoryId:String):LiveData<MutableList<ItemsModel>>{
//        return repository.loadItemCategory(categoryId)
//    }
    private val _categories = MutableLiveData<List<CategoryModel>>()
    val categories: MutableLiveData<List<CategoryModel>> = _categories

    private val _popularItems = MutableLiveData<List<ItemsModel>>()
    val popularItems: MutableLiveData<List<ItemsModel>> = _popularItems

    private val _categoryItems = MutableLiveData<List<ItemsModel>>()
    val categoryItems: LiveData<List<ItemsModel>> = _categoryItems

    fun loadCategory() {
        viewModelScope.launch {
            try {
                val categoryList = repository.getCategories()
                categories.value = categoryList
            } catch (e: Exception) {
                // Handle error - có thể emit empty list hoặc show error message
                categories.value = emptyList()
            }
        }
    }

    fun loadPopular() {
        viewModelScope.launch {
            try {
                val popularList = repository.getPopular()
                popularItems.value = popularList
            } catch (e: Exception) {
                // Handle error
                popularItems.value = emptyList()
            }
        }
    }

    fun loadItems(categoryName: String) { //
        viewModelScope.launch {
            try {
                val itemsList = repository.getItemsByCategory(categoryName) //

                _categoryItems.value = itemsList //
            } catch (e: Exception) {
                // Handle error
                _categoryItems.value = emptyList() //
                Log.e("MainViewModel", "Error loading items by category: ${e.message}", e) //
            }
        }
    }
}