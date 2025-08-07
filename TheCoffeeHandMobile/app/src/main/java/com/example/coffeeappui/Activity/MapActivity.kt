package com.example.coffeeappui.Activity

import android.Manifest
import android.content.pm.PackageManager
import android.location.Geocoder
import android.os.Bundle
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat
import com.example.coffeeappui.R
import com.google.android.gms.location.FusedLocationProviderClient
import org.osmdroid.config.Configuration
import org.osmdroid.tileprovider.tilesource.TileSourceFactory
import org.osmdroid.util.BoundingBox
import org.osmdroid.util.GeoPoint
import org.osmdroid.views.MapView
import org.osmdroid.views.overlay.Marker
import org.osmdroid.views.overlay.Polyline
import java.io.IOException
import java.util.*

class MapActivity : AppCompatActivity() {

    private lateinit var map: MapView
    private lateinit var storePoint: GeoPoint
    private lateinit var startPoint: GeoPoint
    private lateinit var mapController: org.osmdroid.api.IMapController
    //private lateinit var fusedLocationClient: FusedLocationProviderClient

    private var STORE_LOCATION = "12, Nguyễn Du, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh"
    private var START_LOCATION = "50, Nguyễn Du, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh"

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        Configuration.getInstance().userAgentValue = applicationContext.packageName
        setContentView(R.layout.activity_map)

        // Bind map
        map = findViewById(R.id.map)
        map.setTileSource(TileSourceFactory.MAPNIK)
        map.setMultiTouchControls(true)

        // **Chặn wrap ngang/dọc để không thấy cả lục địa lặp lại**
        map.setHorizontalMapRepetitionEnabled(false)
        map.setVerticalMapRepetitionEnabled(false)

        // Lấy address từ Intent nếu có
//        intent.getStringExtra("address")?.let {
//            if (it.isNotBlank()) STORE_LOCATION = it.trim()
//        }
        mapController = map.controller
        mapController.setZoom(15.0)

        // Check quyền
        checkPermissions()
    }

    private fun checkPermissions() {
        val permissions = arrayOf(
            Manifest.permission.ACCESS_FINE_LOCATION,
            Manifest.permission.ACCESS_COARSE_LOCATION,
            Manifest.permission.INTERNET
        )
        val missing = permissions.filter {
            ContextCompat.checkSelfPermission(this, it) != PackageManager.PERMISSION_GRANTED
        }

        if (missing.isNotEmpty()) {
            ActivityCompat.requestPermissions(this, missing.toTypedArray(), 1001)
        } else {
            initializeMap()
        }
    }

    override fun onRequestPermissionsResult(
        requestCode: Int, permissions: Array<out String>, grantResults: IntArray
    ) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults)
        if (requestCode == 1001 && grantResults.all { it == PackageManager.PERMISSION_GRANTED }) {
            initializeMap()
        } else {
            Toast.makeText(this, "Không đủ quyền, không thể hiển thị bản đồ", Toast.LENGTH_LONG).show()
        }
    }

    private fun initializeMap() {
        val geocoder = Geocoder(this, Locale.getDefault())

        try {
            // Geocode START_LOCATION
            val startList = geocoder.getFromLocationName(START_LOCATION, 1)
            if (startList.isNullOrEmpty()) {
                Toast.makeText(this, "Không tìm thấy START_LOCATION", Toast.LENGTH_LONG).show()
                return
            }
            startPoint = GeoPoint(startList[0].latitude, startList[0].longitude)

            // Geocode STORE_LOCATION
            val storeList = geocoder.getFromLocationName(STORE_LOCATION, 1)
            if (storeList.isNullOrEmpty()) {
                Toast.makeText(this, "Không tìm thấy STORE_LOCATION", Toast.LENGTH_LONG).show()
                return
            }
            storePoint = GeoPoint(storeList[0].latitude, storeList[0].longitude)

            // Hiển thị marker START
            val startMarker = Marker(map).apply {
                position = startPoint
                title = "Điểm xuất phát"
                setAnchor(Marker.ANCHOR_CENTER, Marker.ANCHOR_BOTTOM)
            }
            map.overlays.add(startMarker)

            // Hiển thị marker STORE
            val storeMarker = Marker(map).apply {
                position = storePoint
                title = "Cửa hàng"
                setAnchor(Marker.ANCHOR_CENTER, Marker.ANCHOR_BOTTOM)
            }
            map.overlays.add(storeMarker)

            // Vẽ polyline nối 2 điểm
            val polyline = Polyline().apply {
                setPoints(listOf(startPoint, storePoint))
                width = 5f
                color = android.graphics.Color.BLUE
            }
            map.overlays.add(polyline)

            // Căn khung hình để hiển thị cả hai điểm
            fitToBounds(startPoint, storePoint)

            // Làm mới bản đồ
            map.invalidate()

        } catch (e: IOException) {
            e.printStackTrace()
            Toast.makeText(this, "Lỗi geocode: ${e.message}", Toast.LENGTH_LONG).show()
        }
    }

    private fun fitToBounds(p1: GeoPoint, p2: GeoPoint) {
        val latDiff = kotlin.math.abs(p1.latitude  - p2.latitude)
        val lonDiff = kotlin.math.abs(p1.longitude - p2.longitude)

        if (latDiff < 0.001 && lonDiff < 0.001) {
            // quá gần nhau → zoom cố định level ~17 (phù hợp thành phố)
            val midLat = (p1.latitude  + p2.latitude ) / 2
            val midLon = (p1.longitude + p2.longitude) / 2
            map.controller.setCenter(GeoPoint(midLat, midLon))
            map.controller.setZoom(17.0)
        } else {
            // khoảng cách đủ lớn → fit chính xác
            val north = maxOf(p1.latitude, p2.latitude)
            val south = minOf(p1.latitude, p2.latitude)
            val east  = maxOf(p1.longitude, p2.longitude)
            val west  = minOf(p1.longitude, p2.longitude)
            map.zoomToBoundingBox(BoundingBox(north, east, south, west), true)
        }
    }

    override fun onResume() {
        super.onResume()
        map.onResume()
    }

    override fun onPause() {
        super.onPause()
        map.onPause()
    }
}
