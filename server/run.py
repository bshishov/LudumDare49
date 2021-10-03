import sys
import os

sys.path.append(os.path.dirname(__file__))

from server.__main__ import main

main("0.0.0.0", 6789, "players.db", data_path="./data")
