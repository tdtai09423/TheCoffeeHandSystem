# Sử dụng Webots image chính thức
FROM cyberbotics/webots:latest

# Cài đặt các thư viện cần thiết để chạy Webots headless
RUN apt-get update && apt-get install -y \
    xvfb \
    libgl1-mesa-glx \
    libglib2.0-0 \
    libxkbcommon-x11-0 \
    libxcb-xinerama0 \
    libxcb-shape0 \
    libxcb-randr0 \
    libxcb-shm0 \
    libxcb-xfixes0 \
    libxcb-sync1 \
    libxcb-icccm4 \
    libxcb-keysyms1 \
    libxcb-image0 \
    libxcb-render-util0 \
    libxcb-xinerama0 \
    libxcb-util1 \
    libx11-xcb1 \
    && rm -rf /var/lib/apt/lists/*

# Thiết lập biến môi trường để vô hiệu hóa giao diện đồ họa Qt
ENV QT_QPA_PLATFORM=xcb
ENV DISPLAY=:0
ENV WEBOTS_STREAM=TRUE

# Đặt thư mục làm việc
WORKDIR /usr/src/app

# Sao chép toàn bộ thư mục robot-arm vào container
COPY robot-arm /usr/src/app/

# Mở cổng Webots Streaming Server
EXPOSE 1234

# Chạy Webots với file thế giới của bạn
CMD ["xvfb-run", "--server-args=-screen 0 1024x768x24", "webots", "--no-rendering", "--stream", "--port=1234", "worlds/summit_xl_steel.wbt"]
