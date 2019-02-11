/*
 * Description:     A basic PONG simulator
 * Author:           Jon Segeren
 * Date:            Feb 07 2019
 */

#region libraries

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Media;

#endregion

namespace Pong
{
    public partial class Form1 : Form
    {
        #region global values

        //graphics objects for drawing
        SolidBrush drawBrush = new SolidBrush(Color.White);
        Font drawFont = new Font("Courier New", 10);

        // Sounds for game
        SoundPlayer scoreSound = new SoundPlayer(Properties.Resources.score);
        SoundPlayer collisionSound = new SoundPlayer(Properties.Resources.collision);

        //determines whether a key is being pressed or not
        Boolean aKeyDown, zKeyDown, jKeyDown, mKeyDown;

        // check to see if a new game can be started
        Boolean newGameOk = true;

        //ball directions, speed, and rectangle
        Boolean ballMoveRight = false;
        Boolean ballMoveDown = true;
        int ballSpeed = 2;
        Rectangle ball;
        Rectangle bottomWall;

        //paddle speeds and rectangles
        int paddleSpeed = 2;
        Rectangle p1, p2, cpu;
        const int CPU_PADDLE_SPEED = 4;
        Boolean cpuDown = true;

        //player and game scores
        int player1Score = 0;
        int player2Score = 0;
        int gameWinScore = 5;  // number of points needed to win game

        #endregion

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.A:
                    aKeyDown = true;
                    break;
                case Keys.Z:
                    zKeyDown = true;
                    break;
                case Keys.J:
                    jKeyDown = true;
                    break;
                case Keys.M:
                    mKeyDown = true;
                    break;
                case Keys.Y:
                case Keys.Space:
                    if (newGameOk)
                    {
                        SetParameters();
                    }
                    break;
                case Keys.N:
                    if (newGameOk)
                    {
                        Close();
                    }
                    break;
            }
        }

        // -- YOU DO NOT NEED TO MAKE CHANGES TO THIS METHOD
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            //check to see if a key has been released and set its KeyDown value to false if it has
            switch (e.KeyCode)
            {
                case Keys.A:
                    aKeyDown = false;
                    break;
                case Keys.Z:
                    zKeyDown = false;
                    break;
                case Keys.J:
                    jKeyDown = false;
                    break;
                case Keys.M:
                    mKeyDown = false;
                    break;
            }
        }

        /// <summary>
        /// sets the ball and paddle positions for game start
        /// </summary>
        private void SetParameters()
        {

            if (newGameOk)
            {
                player1Score = player2Score = 0;
                newGameOk = false;
                startLabel.Visible = false;
                gameUpdateLoop.Start();
            }


            const int PADDLE_EDGE = 20;  // buffer distance between screen edge and paddle            

            p1.Width = p2.Width = cpu.Width = 10;    //height for both paddles set the same
            p1.Height = p2.Height = cpu.Height = 40;  //width for both paddles set the same

            //p1 starting position
            p1.X = PADDLE_EDGE;
            p1.Y = this.Height / 2 - p1.Height / 2;

            //p2 starting position
            p2.X = this.Width - PADDLE_EDGE - p2.Width;
            p2.Y = this.Height / 2 - p2.Height / 2;

            //cpu start position
            cpu.X = this.Width / 2 - 5;
            cpu.Y = 0;
            //bottom wall
            bottomWall.X = 0;
            bottomWall.Y = this.Height;
            bottomWall.Width = this.Width;
            bottomWall.Height = 20;


            ball.Width = ball.Height = 10;
            ball.X = this.Width / 2 - ball.Width / 2;
            ball.Y = this.Height / 2 - ball.Height / 2;

            Thread.Sleep(2000);
        }

        /// <summary>
        /// This method is the game engine loop that updates the position of all elements
        /// and checks for collisions.
        /// </summary>
        private void gameUpdateLoop_Tick(object sender, EventArgs e)
        {
            #region update ball position

            if (ballMoveRight == true)
            {
                ball.X = ball.X + ballSpeed;
            }
            else
            {
                ball.X = ball.X - ballSpeed;
            }
            if (ballMoveDown == true)
            {
                ball.Y = ball.Y + ballSpeed;
            }
            else
            {
                ball.Y = ball.Y - ballSpeed;
            }
            #endregion

            #region update paddle positions

            if (aKeyDown == true && p1.Y > 5)
            {
                p1.Y = p1.Y - paddleSpeed;
            }

            if (zKeyDown == true && p1.Y < this.Height - p1.Height - 5)
            {
                p1.Y = p1.Y + paddleSpeed;
            }

            if (jKeyDown == true && p2.Y > 5)
            {
                p2.Y = p2.Y - paddleSpeed;
            }
            if (mKeyDown == true && p2.Y < this.Height - p2.Height - 5)
            {
                p2.Y = p2.Y + paddleSpeed;
            }

            if (cpuDown == false)
            {
                cpu.Y = cpu.Y - CPU_PADDLE_SPEED;

            }
            if (cpuDown == true)
            {
                cpu.Y = cpu.Y + CPU_PADDLE_SPEED;
            }

            if (cpu.Y <= -1)
            {
                cpuDown = true;
            }
            if (cpu.Y + cpu.Height >= this.Height + 1)
            {
                cpuDown = false;
            }
            #endregion

            #region ball collision with top and bottom lines

            if (ball.Y <= 0) // if ball hits top line
            {

                ballMoveDown = true;
                collisionSound.Play();
            }
            else if (ball.Y > this.Height - ball.Width)
            {
                ballMoveDown = false;
                collisionSound.Play();
            }


            #endregion

            #region ball collision with paddles


            if (p1.IntersectsWith(ball) || p2.IntersectsWith(ball) || cpu.IntersectsWith(ball))
            {
                ballMoveRight = !ballMoveRight;
                collisionSound.Play();
                ballSpeed++;
                paddleSpeed = paddleSpeed + 2;
            }

            #endregion

            #region ball collision with side walls (point scored)

            if (ball.X < 0 - ball.Width - 3)  // ball hits left wall logic
            {

                player2Score++;
                scoreSound.Play();
                paddleSpeed = 2;
                ballSpeed = 2;
                if (player2Score == gameWinScore)
                {
                    GameOver("Player 2 Wins!");
                }
                else
                {
                    SetParameters();
                    ballMoveRight = true;
                }
            }

            if (ball.X > this.Width + 3)
            {
                player1Score++;
                scoreSound.Play();
                paddleSpeed = 2;
                ballSpeed = 2;
                if (player1Score == gameWinScore)
                {
                    GameOver("Player 1 Wins!"); //make this string also 
                }
                else
                {
                    SetParameters();
                    ballMoveRight = false;
                }

            }



            #endregion

            //refresh the screen, which causes the Form1_Paint method to run
            this.Refresh();
        }

        /// <summary>
        /// Displays a message for the winner when the game is over and allows the user to either select
        /// to play again or end the program
        /// </summary>
        /// <param name="winner">The player name to be shown as the winner</param>
        private void GameOver(string winner)
        {
            newGameOk = true;
            gameUpdateLoop.Stop();
            startLabel.Visible = true;
            startLabel.Text = winner;
            this.Refresh();
            Thread.Sleep(2000);
            startLabel.Text = "Press Space To Play";

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(drawBrush, p1);
            e.Graphics.FillRectangle(drawBrush, p2);
            e.Graphics.FillRectangle(drawBrush, cpu);
            e.Graphics.FillEllipse(drawBrush, ball);
            e.Graphics.DrawString(player1Score + "", drawFont, drawBrush, this.Width / 2 - 70, 30);
            e.Graphics.DrawString(player2Score + "", drawFont, drawBrush, this.Width / 2 + 70, 30);
        }

    }
}
