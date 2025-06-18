from classes.RobotArm import RobotArm

TIME_STEP = 32
ROTATING_TO_CAN, DOWN, GRASPING, LIFT = range(4)

def main():
    arm = RobotArm()
    counter = 0
    state = ROTATING_TO_CAN
    
    while arm.robot.step(TIME_STEP) != -1:
        if counter <= 0:
            if state == ROTATING_TO_CAN:
                print("Rotate to can")
                arm.setToReadyState()  # Giả sử hành động này đưa cánh tay đến vị trí sẵn sàng.
                state = DOWN
                counter = 80  # Thời gian chờ trước khi thực hiện hành động tiếp theo
            
            elif state == DOWN:
                print("Moving down to grasp the coffee")
                arm.setToCoffee()  # Giả sử hành động này di chuyển cánh tay xuống để lấy cà phê.
                state = GRASPING
                counter = 80 # Thời gian chờ sau khi hạ xuống
            
            elif state == GRASPING:
                print("Grasping the coffee can")
                arm.setToReadyState()  # Giả sử đây là hành động nắm lấy cà phê.
                state = LIFT
                counter = 50  # Thời gian chờ sau khi nắm

        
        counter -= 1

if __name__ == "__main__":
    main()
