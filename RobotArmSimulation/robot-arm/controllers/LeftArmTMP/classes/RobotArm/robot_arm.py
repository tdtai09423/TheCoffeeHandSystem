from controller import Robot

TIME_STEP = 32

class RobotArm:
    def __init__(self):
        self.robot = Robot()
        self.speed = 1.0
        
        # Retrieve motors
        self.hand_motors = [
            self.robot.getDevice("finger_1_joint_1"),
            self.robot.getDevice("finger_2_joint_1"),
            self.robot.getDevice("finger_middle_joint_1")
        ]
        self.ur_motors = {
            "shoulder_pan_joint": self.robot.getDevice("shoulder_pan_joint"),
            "shoulder_lift_joint": self.robot.getDevice("shoulder_lift_joint"),
            "elbow_joint": self.robot.getDevice("elbow_joint"),
            "wrist_1_joint": self.robot.getDevice("wrist_1_joint"),
            "wrist_2_joint": self.robot.getDevice("wrist_2_joint"),
            "wrist_3_joint": self.robot.getDevice("wrist_3_joint")
        }
        
        for motor in self.ur_motors.values():
            motor.setVelocity(self.speed)
        
        # Retrieve sensors
        self.distance_sensor = self.robot.getDevice("distance sensor")
        self.distance_sensor.enable(TIME_STEP)
        
        self.position_sensor = self.robot.getDevice("wrist_1_joint_sensor")
        self.position_sensor.enable(TIME_STEP)
    
    def grasping(self):
        for motor in self.hand_motors:
            motor.setPosition(0.85)
    
    def release(self):
        for motor in self.hand_motors:
            motor.setPosition(motor.getMinPosition())
    
    def setAngle(self, joint_name, angle, speed=1.0):
        if joint_name in self.ur_motors:
            self.ur_motors[joint_name].setVelocity(speed)
            self.ur_motors[joint_name].setPosition(angle)
    
    def is_object_nearby(self):
        return self.distance_sensor.getValue() < 500

    def setToReadyState(self):
        self.setAngle("shoulder_pan_joint", -1.5, 1.5)
        self.setAngle("shoulder_lift_joint", -1.5, 1)
        self.setAngle("elbow_joint", 2.5, 3.14)
        self.setAngle("wrist_1_joint", -3, 2)
        self.setAngle("wrist_3_joint", 2, 1.7)

    def setToWater(self):
        self.setAngle("shoulder_pan_joint", -1.3)
        self.setAngle("shoulder_lift_joint", 0)
        self.setAngle("elbow_joint", 0, 1.5)
        self.setAngle("wrist_1_joint", 0, 1.5)
        self.setAngle("wrist_3_joint", 0)
    
    def setToIceMaking(self):
        self.setAngle("shoulder_pan_joint", -1.8)
        self.setAngle("shoulder_lift_joint", -0.3, 2)
        self.setAngle("elbow_joint", 1.3, 2)
        self.setAngle("wrist_1_joint", -3, 3)
        self.setAngle("wrist_3_joint", 2, 1.7)

    def setToMilk(self):
        self.setAngle("shoulder_pan_joint", -2.2)
        self.setAngle("shoulder_lift_joint", -0.5)
        self.setAngle("elbow_joint", 1.4)
        self.setAngle("wrist_1_joint", -3)
        self.setAngle("wrist_3_joint", 2.1)

    def setToCoffee(self):
        self.setAngle("shoulder_pan_joint", -2.57, 2)
        self.setAngle("shoulder_lift_joint", 0, 0.6)
        self.setAngle("elbow_joint", 0.2, 0.9)
        self.setAngle("wrist_1_joint", -0.3)
        self.setAngle("wrist_3_joint", 0)

    def setToSugar(self):
        self.setAngle("shoulder_pan_joint", 1, 2)
        self.setAngle("shoulder_lift_joint", 0, 0.8)
        self.setAngle("elbow_joint", 0, 1.2)
        self.setAngle("wrist_1_joint", 0, 2)
        self.setAngle("wrist_3_joint", 0)

    def setToEggWhisker(self):
        self.setAngle("shoulder_pan_joint", 1.5, 2)
        self.setAngle("shoulder_lift_joint", 0, 0.8)
        self.setAngle("elbow_joint", 0, 1.2)
        self.setAngle("wrist_1_joint", 0, 2)
        self.setAngle("wrist_3_joint", 0)


