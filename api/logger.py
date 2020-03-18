""" Project         : Logger     	
    File            : logger.py
    Programmer      : Wheeping Angels, Frederic Chappuis
    First Version   : July 15, 2019
    Description     : This will use python own logging module to keep
                      logs across the app
                      https://docs.python.org/3/howto/logging-cookbook.html
"""
from logging.handlers import RotatingFileHandler

import logging


class Logger:
    logger = ""
    log_file = ""
    max_file_size = 50 * 1024  # should be a 50K file
    backup_count = 3

    def __init__(self, logger_name, logger_file="error.log"):
        self.logger = logging.getLogger(logger_name)
        self.log_file = logger_file
        self.logger.setLevel(logging.DEBUG)

        # to get max log file size
        # https://stackoverflow.com/questions/24505145/how-to-limit-log-file-size-in-python/24505345
        # https://docs.python.org/3/library/logging.handlers.html#logging.handlers.RotatingFileHandler
        fh = RotatingFileHandler(
            self.log_file, maxBytes=self.max_file_size, backupCount=self.backup_count
        )
        formatter = logging.Formatter(
            "%(asctime)s - %(name)s - %(levelname)s - %(message)s"
        )
        fh.setFormatter(formatter)
        self.logger.addHandler(fh)

    # The next 5 function are just to inditcate
    # the level of logging with the message
    def log_info(self, message):
        self.logger.info(message)

    def log_debug(self, message):
        self.logger.debug(message)

    def log_warning(self, message):
        self.logger.warning(message)

    def log_error(self, message):
        self.logger.error(message)

    def log_critical(self, message):
        self.logger.critical(message)
