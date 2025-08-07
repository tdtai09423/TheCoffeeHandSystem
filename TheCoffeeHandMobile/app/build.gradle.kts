plugins {
    alias(libs.plugins.android.application)
    alias(libs.plugins.kotlin.android)
    id("kotlin-parcelize")
    id("com.google.gms.google-services")
}

android {
    namespace = "com.example.coffeeappui"
    compileSdk = 35

    defaultConfig {
        applicationId = "com.example.coffeeappui"
        minSdk = 24
        targetSdk = 35
        versionCode = 1
        versionName = "1.0"

        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
    }

    buildTypes {
        release {
            isMinifyEnabled = false
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
        }
    }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_11
        targetCompatibility = JavaVersion.VERSION_11
    }
    kotlinOptions {
        jvmTarget = "11"
    }
    buildFeatures {
        viewBinding = true
    }
}

dependencies {

    implementation(libs.appcompat)
    implementation(libs.material)
    implementation(libs.activity)
    implementation(libs.constraintlayout)
    implementation(libs.firebase.database.ktx)
    implementation(libs.firebase.auth.ktx)
    implementation(libs.credentials)
    implementation(libs.credentials.play.services.auth)
    implementation(libs.googleid)
    implementation(libs.firebase.auth)
    implementation(libs.firebase.messaging.ktx)
    implementation(libs.transport.api)
    implementation(libs.transport.api)
    testImplementation(libs.junit)
    androidTestImplementation(libs.ext.junit)
    androidTestImplementation(libs.espresso.core)

    implementation(libs.glide)
    implementation(libs.gson)

    // Firebase BOM để quản lý version
    implementation(platform(libs.firebase.bom.v3270))

    // Firebase Auth
    implementation(libs.google.firebase.auth)

    // Google Sign-In
    implementation(libs.play.services.auth.v2070)

    // Firebase Messaging (cho FCM token)
    implementation(libs.google.firebase.messaging)

    // Coroutines (nếu chưa có)
    implementation(libs.kotlinx.coroutines.android)

    implementation(libs.lifecycle.viewmodel.ktx)

    implementation(libs.recyclerview)

    implementation(libs.okhttp)

    // OSMDroid core và Android
    implementation(libs.osmdroid.android)
    // Plugin hỗ trợ MyLocation overlay (dùng để hiển thị vị trí)
    implementation(libs.osmdroid.wms)
    // Cung cấp FusedLocationProviderClient (nếu bạn dùng)
    implementation(libs.play.services.location)
    // Google Play Services Location (để dùng FusedLocationProviderClient)
    // AndroidX AppCompat (thường có sẵn trong template)
//    implementation 'androidx.appcompat:appcompat:1.5.1'
}