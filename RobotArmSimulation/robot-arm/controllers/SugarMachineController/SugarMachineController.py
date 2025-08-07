from machine.SugarMachine import SugarMachine
import pika
import threading
import json
import time

TIME_STEP = 32

def rabbitmq_listener(sugar_machine):
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

            if machine == "sugar machine":
                process_command(sugar_machine, activity_id, machine, mode, parameters, sequence, ch)
        except json.JSONDecodeError:
            print("[Error] Invalid message format")

    # Kết nối tới RabbitMQ
    connection = pika.BlockingConnection(pika.URLParameters(
        "amqps://jbkouopo:C4l6etM1edUdQ3tKsUAVeMn5CtkFfBSD@toucan.lmq.cloudamqp.com/jbkouopo"
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
    
    print("[sugar Machine] Waiting for messages from send_command_exchange...")
    channel.start_consuming()


def process_command(sugar_machine, activity_id, machine, mode, parameters, sequence, channel):
    """Xử lý lệnh gửi đến sugar Machine"""
    if machine == "sugar machine":
        #print(f"{machine} in mode {mode} with parameters: {parameters} and sequence {sequence}")

        time.sleep(5)

        response_message = {"activity_id": activity_id, "status": "done", "sequence": sequence}
        channel.basic_publish(
            exchange='',
            routing_key='response_queue',
            body=json.dumps(response_message)
        )
    


def main():
    sugar_machine = SugarMachine()

    threading.Thread(target=rabbitmq_listener, args=(sugar_machine,), daemon=True).start()

    while sugar_machine.robot.step(TIME_STEP) != -1:
        pass


if __name__ == "__main__":
    main()
