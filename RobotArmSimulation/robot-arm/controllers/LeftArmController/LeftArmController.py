import json
import pika
import threading
import time
from classes.RobotArm import RobotArm

TIME_STEP = 32
connection = pika.BlockingConnection(pika.URLParameters(
    "amqps://jrlfzsrv:p64MW92lLWnfU4XtfHaCjYbR7FNj3STv@turkey.rmq.cloudamqp.com/jrlfzsrv"
))
channel = connection.channel()
arm = RobotArm()  # Khởi tạo đối tượng RobotArm


def rabbitmq_listener():
    """
    RabbitMQ Consumer Function
    """
    def callback(ch, method, properties, body):
        message = body.decode()
        process_command(message, ch)

    channel.queue_declare(queue='robot_arm', durable=True)

    channel.basic_consume(queue='robot_arm', on_message_callback=callback, auto_ack=True)

    print("[INFO] Waiting for messages from 'robot_arm'...")
    channel.start_consuming()


def process_command(machine, channel):
    """
    Process command sent to Robot Arm
    """
    machine = machine.lower()

    if machine == "coffee machine":
        print("[RobotArm] Moving to Coffee Machine")
        arm.setToReadyState()
        time.sleep(3)
        arm.setToCoffee()  # Điều khiển cánh tay tới vị trí Coffee Machine
        time.sleep(3)  # Cho cánh tay thời gian để di chuyển đến vị trí
        send_status_to_mq(machine, "done", channel)
        
    elif machine == "egg whisker machine":
        print("[RobotArm] Moving to Egg Whisker Machine")
        arm.setToReadyState()
        time.sleep(3)
        arm.setToEggWhisker()  # Điều khiển cánh tay tới vị trí Egg Whisker Machine
        time.sleep(3)
        send_status_to_mq(machine, "done", channel)
        
    elif machine == "milk machine":
        print("[RobotArm] Moving to Milk Machine")
        arm.setToReadyState()
        time.sleep(3)
        arm.setToMilk()  # Điều khiển cánh tay tới vị trí Milk Machine
        time.sleep(3)
        send_status_to_mq(machine, "done", channel)
        
    elif machine == "sugar machine":
        print("[RobotArm] Moving to Sugar Machine")
        arm.setToReadyState()
        time.sleep(3)
        arm.setToSugar()  # Điều khiển cánh tay tới vị trí Sugar Machine
        time.sleep(3)
        send_status_to_mq(machine, "done", channel)
        
    elif machine == "water machine":
        print("[RobotArm] Moving to Water Machine")
        arm.setToReadyState()
        time.sleep(3)
        arm.setToWater()  # Điều khiển cánh tay tới vị trí Water Machine
        time.sleep(3)
        send_status_to_mq(machine, "done", channel)
        
    elif machine == "ice making machine":
        print("[RobotArm] Moving to Ice Making Machine")
        arm.setToReadyState()
        time.sleep(3)
        arm.setToIceMaking()  # Điều khiển cánh tay tới vị trí Ice Making Machine
        time.sleep(3)
        send_status_to_mq(machine, "done", channel)

    elif machine == "done":
        print("[RobotArm] Moving to Ready State")
        arm.setToReadyState()
        time.sleep(3)
        send_status_to_mq(machine, "done", channel)
    


def send_status_to_mq(machine, status, channel):
    """
    Send status to the message queue 'robot_arm_status'
    """
    try:
        message = json.dumps({"machine": machine, "status": status})
        channel.basic_publish(exchange='', routing_key='robot_arm_status', body=message)
        print(f"[PUSH TO MQ] Sent status to 'robot_arm_status': {message}")
    except Exception as e:
        print(f"[ERROR] Failed to send status to 'robot_arm_status': {e}")


def main():
    threading.Thread(target=rabbitmq_listener, daemon=True).start()

    while arm.robot.step(TIME_STEP) != -1:
        pass


if __name__ == "__main__":
    main()
