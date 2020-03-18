"""empty message

Revision ID: 205f8489bc68
Revises: 7953a2dd0a0e
Create Date: 2020-03-18 15:42:34.332031

"""
from alembic import op
import sqlalchemy as sa


# revision identifiers, used by Alembic.
revision = '205f8489bc68'
down_revision = '7953a2dd0a0e'
branch_labels = None
depends_on = None


def upgrade():
    # ### commands auto generated by Alembic - please adjust! ###
    op.add_column('map_images', sa.Column('center_point', sa.String(), nullable=True))
    op.drop_column('map_images', 'bottom_left_corner')
    # ### end Alembic commands ###


def downgrade():
    # ### commands auto generated by Alembic - please adjust! ###
    op.add_column('map_images', sa.Column('bottom_left_corner', sa.VARCHAR(), autoincrement=False, nullable=True))
    op.drop_column('map_images', 'center_point')
    # ### end Alembic commands ###
