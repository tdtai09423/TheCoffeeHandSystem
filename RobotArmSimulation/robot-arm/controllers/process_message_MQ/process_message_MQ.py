from classes.RobotArm import RobotArm
from collections import deque
import pika
import time
import threading
import json
import re

TIME_STEP = 32
connection = pika.BlockingConnection(pika.URLParameters(
    "amqps://jbkouopo:C4l6etM1edUdQ3tKsUAVeMn5CtkFfBSD@toucan.lmq.cloudamqp.com/jbkouopo"
))
channel = connection.channel()

class MachineMaster:
    def __init__(self):
        self.machine_status = {}



def send_command_to_mq(machine, mode, parameters, sequence, activity_id):
    try:
        # Khai báo Exchange và hàng đợi phản hồi
        channel.exchange_declare(exchange='send_command_exchange', exchange_type='fanout')
        response_queue = channel.queue_declare(queue='response_queue', durable=True).method.queue

        # Tạo message dưới dạng JSON
        message = json.dumps({
            "machine": machine,
            "mode": mode,
            "parameters": parameters,
            "sequence": sequence,
            "activity_id": activity_id
        })

        # Gửi message lên Exchange
        channel.basic_publish(
            exchange='send_command_exchange',
            routing_key='',
            body=message
        )

        print(f"[PUSH TO MQ] Sent command to '{machine}' | Mode: {mode} | Parameters: {parameters} | Sequence: {sequence}")

        # Đợi phản hồi từ response_queue
        for method_frame, properties, body in channel.consume(response_queue, inactivity_timeout=60):
            if body:
                response = json.loads(body.decode())
                if response.get('activity_id') == activity_id:
                    channel.basic_ack(method_frame.delivery_tag)
                    #connection.close()
                    return response
            else:
                break

        #connection.close()
        return None  # Không nhận được phản hồi

    except Exception as e:
        print(f"[ERROR] Failed to push command to MQ: {e}")
        return None


def process_message(message, machine_controller):
    try:
        data = json.loads(message)
        actions = data.get('actions', [])
        activity_id = data.get('activity_id')

        for action in sorted(actions, key=lambda x: x['sequence']):
            machine = action['machine']
            mode = action['mode']
            parameters = action['parameters']
            sequence = action['sequence']

            send_message_to_robot_arm(machine)

            # check = wait_for_robot_arm(machine.lower())
            if wait_for_robot_arm(machine.lower()):

                # Gửi lệnh và chờ phản hồi
                response = send_command_to_mq(machine, mode, parameters, sequence, activity_id)

                if response:
                    status = response.get("status")
                    if status == "done":
                        print(f"[INFO] Command '{machine}' completed successfully.")
                    elif status == "fail":
                        print(f"[ERROR] Command '{machine}' failed. Retrying...")
                        # Thử lại gửi tin nhắn nếu thất bại
                        send_command_to_mq(machine, mode, parameters, sequence, activity_id)
                else:
                    print(f"[INFO] Response received from '{machine}': {response}.")
        
        #print(f"[ORDER] Order '{activity_id}' completed successfully.")
        send_message_to_robot_arm("done")
        if wait_for_robot_arm(machine.lower()):
            print(f"[ORDER] Order '{activity_id}' completed successfully.")


    except json.JSONDecodeError:
        print("Invalid message format")


def get_order_id(input_string: str) -> str:
    pattern = r"MK_[^_]+_([a-f0-9\-]+)"
    match = re.search(pattern, input_string)

    if match:
        return match.group(1)
    return None


# def send_complete_order_message(order_id: str):
#     """Gửi yêu cầu hoàn thành order lên API"""
#     try:
#         # Địa chỉ URL của API để hoàn thành order
#         url = f"http://localhost:7099/api/orders/{order_id}/complete"  # Thay 'localhost:5000' bằng địa chỉ chính xác của bạn

#         # Gửi yêu cầu POST tới API
#         response = requests.post(url)

#         # Kiểm tra phản hồi từ server
#         if response.status_code == 200:
#             print("[INFO] Order completed successfully.")
#             return True
#         else:
#             print(f"[ERROR] Failed to complete order. Status Code: {response.status_code}")
#             print("Response:", response.json())
#             return False

#     except Exception as e:
#         print(f"[ERROR] An error occurred while completing order: {e}")
#         return False

def send_message_to_robot_arm(machine):
    """Gửi machine lên hàng đợi 'robot_arm'"""
    try:
        channel.queue_declare(queue='robot_arm', durable=True)

        # Gửi message lên hàng đợi 'robot_arm'
        channel.basic_publish(
            exchange='',
            routing_key='robot_arm',
            body=machine
        )

        #connection.close()

    except Exception as e:
        print(f"[ERROR] Failed to push machine list to 'robot_arm' queue: {e}")

def wait_for_robot_arm(machine):
    """Lắng nghe từ hàng đợi 'robot_arm_status' để chắc chắn RobotArm đã ở đúng vị trí"""
    try:
        channel.queue_declare(queue='robot_arm_status', durable=True)

        while True:
            method_frame, properties, body = channel.basic_get(queue='robot_arm_status', auto_ack=True)
            if body:
                response = json.loads(body.decode())
                if response.get("machine") == machine and response.get("status") == "done":
                    #print(f"[INFO] {machine} is ready.")
                    #connection.close()
                    return True  # Trả về True nếu máy đã sẵn sàng
            time.sleep(0.1)

    except Exception as e:
        print(f"[ERROR] Failed to listen from 'robot_arm_status': {e}")
        return False


def rabbitmq_listener(machine_master):
    """RabbitMQ Consumer Function"""
    def callback(ch, method, properties, body):
        message = body.decode()
        print(f"![Received message]: {message}")

        # Xử lý message nhận được
        process_message(message, machine_master)

    channel.queue_declare(queue='machine_queue', durable=True)
    channel.basic_consume(queue='machine_queue', on_message_callback=callback, auto_ack=True)

    print("Waiting for messages from CloudAMQP...")
    channel.start_consuming()


def main():
    machine_master = MachineMaster()
    robot_arm = RobotArm()

    # Chạy RabbitMQ listener trong một luồng riêng biệt
    threading.Thread(target=rabbitmq_listener, args=(machine_master,), daemon=True).start()

    # Vòng lặp liên tục để Webots không bị dừng
    while robot_arm.robot.step(TIME_STEP) != -1:
        pass


if __name__ == "__main__":
    main()
