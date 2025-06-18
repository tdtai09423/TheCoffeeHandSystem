package com.example.coffeeappui.ViewModel

import androidx.lifecycle.LiveData
import androidx.lifecycle.ViewModel
import com.example.coffeeappui.Domain.BannerModel
import com.example.coffeeappui.Repository.MainRepository

class MainViewModel : ViewModel(){
    private val repository = MainRepository()

    fun loadBanner(): LiveData<MutableList<BannerModel>> {
        return repository.loadBanner()
    }


}