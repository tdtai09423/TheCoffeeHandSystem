from controller import Robot

TIME_STEP = 32

block_positions = [[0.37, -2.7, 2.9], [0.52, -2.37, 2.73], [0.74, -1.96, 2.53]]
block_clearings = [[-0.1, -2.78, 2.61], [0.22, -2.48, 2.53], [0.33, -2.18, 2.35]]

JOINTS = ["panda_joint1", "panda_joint2", "panda_joint3", "panda_joint4", "panda_joint5", "panda_joint6", "panda_joint7"]
FINGER = "panda_finger::right"

BLOCK1, BLOCK2, BLOCK3 = 0, 1, 2
OPEN_HAND, CLOSE_HAND = 0, 1

class EggWhiskerMachine:
    def __init__(self):
        self.robot = Robot()
        self.motors = {}
        self.init_motors()

    def init_motors(self):
        for joint in JOINTS:
            self.motors[joint] = self.robot.getDevice(joint)
        self.motors[FINGER] = self.robot.getDevice(FINGER)
        self.hand_control(OPEN_HAND)

    def hand_control(self, command):
        self.motors[FINGER].setPosition(0.02 if command == OPEN_HAND else 0.012)
        self.robot.step(TIME_STEP * 10)

    def move_to_block(self, block):
        self.motors[JOINTS[1]].setPosition(block_positions[block][0])
        self.motors[JOINTS[3]].setPosition(block_positions[block][1])
        self.motors[JOINTS[5]].setPosition(block_positions[block][2])
        self.robot.step(TIME_STEP * 20)

    def move_to_clearing(self, block):
        self.motors[JOINTS[1]].setPosition(block_clearings[block][0])
        self.motors[JOINTS[3]].setPosition(block_clearings[block][1])
        self.motors[JOINTS[5]].setPosition(block_clearings[block][2])
        self.robot.step(TIME_STEP * 20)

    def sequence(self, origin_block, target_block):
        self.move_to_block(origin_block)
        self.move_to_clearing(origin_block)
        self.move_to_clearing(target_block)
        self.move_to_block(target_block)
        self.move_to_clearing(target_block)

    def run(self):
        for _ in range(10):
            self.sequence(BLOCK1, BLOCK3)
            self.sequence(BLOCK2, BLOCK1)
            self.sequence(BLOCK3, BLOCK2)

        self.robot.cleanup()


