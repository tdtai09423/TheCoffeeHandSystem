package com.example.coffeeappui.Domain

import android.os.Parcelable
import kotlinx.parcelize.Parcelize

@Parcelize
data class CategoryModel(
    val id: String,
    val title: String
) : Parcelable