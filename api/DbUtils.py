from sqlalchemy import create_engine
from psycopg2 import IntegrityError


class DbUtils:
    db_string = "postgresql+psycopg2://doctor:wh0@192.0.203.84:5432/capstone"

    @staticmethod
    def addNewPNG(img_name, img, size, corner, rotation):
        db_string = "postgresql+psycopg2://doctor:wh0@192.0.203.84:5432/capstone"
        db = create_engine(db_string)
        conn = db.connect()
        try:
            conn.execute(
                """INSERT INTO images(img_name, img, img_size, bot_left_corner, rotation) VALUES (%s, %s, %s, %s, %s)""",
                (img_name, str(img), size, corner, rotation),
            )
        except Exception as e:
            print(e)
            try:
                conn.execute(
                    """UPDATE images SET img = %s, img_size = %s, bot_left_corner = %s, rotation = %s WHERE img_name = %s;""",
                    (str(img), size, corner, rotation ,img_name),
                )
            except Exception as ee:
                print(ee)
        finally:
            conn.close()
            db.dispose()             

        return

    @staticmethod
    def getPNG(img_name):
        db_string = "postgresql+psycopg2://doctor:wh0@192.0.203.84:5432/capstone"
        db = create_engine(db_string)
        conn = db.connect()
        img = conn.execute(
            "SELECT img_name, img, img_size, ST_AsText(bot_left_corner), rotation FROM images WHERE img_name = %s;",
            img_name,
        )
        conn.close()
        db = None
        
        return img

    @staticmethod
    def getBounds():
        db_string = "postgresql+psycopg2://doctor:wh0@192.0.203.84:5432/capstone"
        db = create_engine(db_string)
        conn = db.connect()
        bounds = conn.execute("SELECT ST_AsText(geom) AS point FROM bounds;")
        conn.close()
        db = None
        return bounds

    @staticmethod
    def getPoints(void):
        db_string = "postgresql+psycopg2://doctor:wh0@192.0.203.84:5432/capstone"
        db = create_engine(db_string)
        conn = db.connect()
        points = conn.execute("SELECT * FROM tour_points;")
        conn.close()
        db = None
        return points

    def addDistance(self, length, width):

        return None
