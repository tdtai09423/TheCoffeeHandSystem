from machine.IceMakingMachine import IceMakingMachine
import pika
import threading
import json
import time
import requests  # Thêm import requests để gửi HTTP request
import random  # Thêm import random để tạo số ngẫu nhiên

THINGSBOARD_TOKEN = "VjsuA90mC4KQb0qQjyHJ"  # Token của ThingsBoard
THINGSBOARD_URL = f"http://thingsboard.cloud/api/v1/{THINGSBOARD_TOKEN}/telemetry"

TIME_STEP = 32

def rabbitmq_listener(ice_making_machine):
    """RabbitMQ Consumer Function"""
    def callback(ch, method, properties, body):
        message = body.decode()
        
        try:
            data = json.loads(message)
            activity_id = data.get("activity_id")
            machine = data.get("machine", "").lower()  # Chuyển tên máy về dạng chữ thường
            mode = data.get("mode")
            parameters = data.get("parameters", {})
            sequence = data.get("sequence", -1)  # Thêm sequence nếu có

            if machine == "ice making machine":
                process_command(ice_making_machine, activity_id, machine, mode, parameters, sequence, ch)
        except json.JSONDecodeError:
            print("[Error] Invalid message format")

    # Kết nối tới RabbitMQ
    connection = pika.BlockingConnection(pika.URLParameters(
        "amqps://jrlfzsrv:p64MW92lLWnfU4XtfHaCjYbR7FNj3STv@turkey.rmq.cloudamqp.com/jrlfzsrv"
    ))
    channel = connection.channel()
    
    # Khai báo hàng đợi
    channel.exchange_declare(exchange='send_command_exchange', exchange_type='fanout')

        # Tạo một hàng đợi tạm thời (không cần đặt tên)
    queue_name = channel.queue_declare(queue='', exclusive=True).method.queue

    # Liên kết hàng đợi với Exchange
    channel.queue_bind(exchange='send_command_exchange', queue=queue_name)
    
    # Lắng nghe hàng đợi
    channel.basic_consume(queue=queue_name, on_message_callback=callback, auto_ack=True)
    
    print("[Ice Making Machine] Waiting for messages from send_command_exchange...")
    channel.start_consuming()


def process_command(egg_whisker_machine, activity_id, machine, mode, parameters, sequence, channel):
    """Xử lý lệnh gửi đến Egg Whisker Machine"""
    if machine == "egg whisker machine":
        #print(f"{machine} in mode {mode} with parameters: {parameters} and sequence {sequence}")

        # Gửi 3 giá trị nhiệt độ ngẫu nhiên lên ThingsBoard
        for i in range(1, 4):
            temperature = random.randint(20, 80)  # Sinh số ngẫu nhiên từ 20 đến 80
            payload = {"temperature": temperature}
            
            try:
                response = requests.post(
                    THINGSBOARD_URL,
                    json=payload,
                    headers={"Content-Type": "application/json"}
                )
                
                if response.status_code == 200:
                    print(f"[ThingsBoard] Egg Whisker Machine - Temperature {i} ({temperature}°C) sent successfully.")
                else:
                    print(f"[ThingsBoard] Egg Whisker Machine - Failed to send Temperature {i}. Response: {response.text}")
            except requests.RequestException as e:
                print(f"[Error] Failed to connect to ThingsBoard: {e}")

        # Sau khi gửi dữ liệu lên ThingsBoard, gửi phản hồi về hàng đợi RabbitMQ
        response_message = {"activity_id": activity_id, "status": "done", "sequence": sequence}
        channel.basic_publish(
            exchange='',
            routing_key='response_queue',
            body=json.dumps(response_message)
        )
    


def main():
    ice_making_machine = IceMakingMachine()

    threading.Thread(target=rabbitmq_listener, args=(ice_making_machine,), daemon=True).start()

    while ice_making_machine.robot.step(TIME_STEP) != -1:
        pass


if __name__ == "__main__":
    main()
