from sqlalchemy import create_engine


class DbUtils:
    db_string = "postgresql+psycopg2://doctor:wh0@192.0.203.84:5432/capstone"

    @staticmethod
    def addNewPNG(img_name, img, size, corner, rotation):
        db_string = "postgresql+psycopg2://doctor:wh0@192.0.203.84:5432/capstone"
        db = create_engine(db_string)
        try:
            db.execute(
                """INSERT INTO images(img_name, img, img_size, bot_left_corner, rotation) VALUES (%s, %s, %s, %s, %s)""",
                (img_name, img, size, corner, rotation),
            )
        except psycopg2.IntegrityError:
            db.execute(
                """DELETE * FROM images WHERE img_name = %s""",
                (img_name),
            )

        db.close()
        db = None
        return

    @staticmethod
    def getPNG(img_name):
        db_string = "postgresql+psycopg2://doctor:wh0@192.0.203.84:5432/capstone"
        db = create_engine(db_string)

        img = db.execute(
            "SELECT img_name, img, img_size, ST_AsText(bot_left_corner), rotation FROM images WHERE img_name = %s;",
            img_name,
        )
        db.close()
        db = None
        return img

    @staticmethod
    def getBounds():
        db_string = "postgresql+psycopg2://doctor:wh0@192.0.203.84:5432/capstone"
        db = create_engine(db_string)

        bounds = db.execute("SELECT ST_AsText(geom) AS point FROM bounds;")
        db.close()
        db = None
        return bounds

    @staticmethod
    def getPoints(void):
        db_string = "postgresql+psycopg2://doctor:wh0@192.0.203.84:5432/capstone"
        db = create_engine(db_string)
        points = db.execute("SELECT * FROM tour_points;")
        db.close()
        db = None
        return points

    def addDistance(self, length, width):

        return None
