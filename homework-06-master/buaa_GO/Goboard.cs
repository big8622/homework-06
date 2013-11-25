/**
 *  Go Applet
 *  1996.11		xinz	written in Java
 *  2001.3		xinz	port to C#
 *  2001.5.10	xinz	file parsing, back/forward
 *  
 *  2013.11     Shone JIN   code style and others
 */

/*************************************************************************
 *  本次作业，英文为注释，汉语为写的不好的地方说明。真正编程中注释最好用英文。*
 *  **********************************************************************/

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Diagnostics; 

namespace GoWinApp //根据code analyze删除了下划线
{

	public enum StoneColor : byte
	{
		black = 0, white = 1 //首字母大写最好
	}


	/**
	 * Board
	 */
	public class GoBoard : System.Windows.Forms.Form
	{
		string [] strLabels; // {"a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t"};

		int nSize;		                //Size of the board, 19 grids per edge
		const int nBoardMargin = 10;	//edge margin of the board
		int nCoodStart = 4;
		const int	nBoardOffset = 20;
		int nEdgeLen = nBoardOffset + nBoardMargin;
		int nTotalGridWidth = 360 + 36;	//the total width of all grids and edge
		int nUnitGridWidth = 22;		//width of each grid
		int nSeq = 0;
		Rectangle rGrid;		    // rectangle for a grid
		StoneColor m_colorToPlay;   // CURRENT stone color
		GoMove m_gmLastMove;	    //last move 
		Boolean bDrawMark;	        
		Boolean m_fAnyKill;	        //is there any killed stone
		Spot [,] Grid;		        //grid locater
        Pen penGrid, penMarkW, penMarkB, penStoneB,penStoneW; //不要一行定义多个
		Brush brStar, brBoard, brBlack, brWhite, m_brMark;
	
        // back and forward step
        // 这里两个量，不太适合放在棋盘类里面，语义上不适合
        // 变量要定义在最近使用的地方，这里一下就能看出是后期新加进去的定义，和原本的思路已经不一样了
        int nFFMove = 10;   //10 more steps shown in each forward 
        int nRewindMove = 10;  // go back 10 steps in each rewind

		GoTree	gameTree;

		/// <summary>
		///    Controllers.
		/// </summary>
		//private System.ComponentModel.Container components; 使用过的变量，删之
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button Rewind;
		private System.Windows.Forms.Button FForward;
		private System.Windows.Forms.Button Save;
		private System.Windows.Forms.Button Open;
		private System.Windows.Forms.Button Back;
		private System.Windows.Forms.Button Forward;

		public GoBoard(int nSize)
		{
			//
			// initialize board form
			//
			InitializeComponent();

			//
			// attributes of size, color and etc.
			//

			this.nSize = nSize;  //number of grid in each edge

			m_colorToPlay = StoneColor.black;

			Grid = new Spot[nSize,nSize];
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
					Grid[i,j] = new Spot();
			penGrid = new Pen(Color.Brown, (float)0.5);
			penStoneW = new Pen(Color.WhiteSmoke, (float)1);
			penStoneB = new Pen(Color.Black, (float)1); //！！代码风格问题。要加空格，这是修改后的
			penMarkW = new Pen(Color.Blue, (float)1);
			penMarkB = new Pen(Color.Beige, (float)1);

			brStar = new SolidBrush(Color.Black);
			brBoard = new SolidBrush(Color.Orange);
			brBlack = new SolidBrush(Color.Black);
			brWhite = new SolidBrush(Color.White);
			m_brMark = new SolidBrush(Color.Red);

			rGrid = new Rectangle(nEdgeLen, nEdgeLen,nTotalGridWidth, nTotalGridWidth);
			strLabels = new string [] {"a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t"};
			gameTree = new GoTree();
		}

		/// <summary>
		///    This part is auto generated
		///    initialize components
		/// </summary>
		private void InitializeComponent()
		{
            this.Open = new System.Windows.Forms.Button();
            this.Save = new System.Windows.Forms.Button();
            this.Rewind = new System.Windows.Forms.Button();
            this.Forward = new System.Windows.Forms.Button();
            this.Back = new System.Windows.Forms.Button();
            this.FForward = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // Open
            // 
            this.Open.Location = new System.Drawing.Point(445, 88);
            this.Open.Name = "Open";
            this.Open.Size = new System.Drawing.Size(56, 23);
            this.Open.TabIndex = 2;
            this.Open.Text = "open";
            this.Open.Click += new System.EventHandler(this.Open_Click);
            // 
            // Save
            // 
            this.Save.Location = new System.Drawing.Point(509, 88);
            this.Save.Name = "Save";
            this.Save.Size = new System.Drawing.Size(56, 23);
            this.Save.TabIndex = 3;
            this.Save.Text = "save";
            this.Save.Click += new System.EventHandler(this.SaveClick);
            // 
            // Rewind
            // 
            this.Rewind.Location = new System.Drawing.Point(509, 56);
            this.Rewind.Name = "Rewind";
            this.Rewind.Size = new System.Drawing.Size(56, 23);
            this.Rewind.TabIndex = 5;
            this.Rewind.Text = "<<";
            this.Rewind.Click += new System.EventHandler(this.Rewind_Click);
            // 
            // Forward
            // 
            this.Forward.Location = new System.Drawing.Point(445, 24);
            this.Forward.Name = "Forward";
            this.Forward.Size = new System.Drawing.Size(56, 23);
            this.Forward.TabIndex = 0;
            this.Forward.Text = ">";
            this.Forward.Click += new System.EventHandler(this.Forward_Click);
            // 
            // Back
            // 
            this.Back.Location = new System.Drawing.Point(509, 24);
            this.Back.Name = "Back";
            this.Back.Size = new System.Drawing.Size(56, 23);
            this.Back.TabIndex = 1;
            this.Back.Text = "<";
            this.Back.Click += new System.EventHandler(this.Back_Click);
            // 
            // FForward
            // 
            this.FForward.Location = new System.Drawing.Point(445, 56);
            this.FForward.Name = "FForward";
            this.FForward.Size = new System.Drawing.Size(56, 23);
            this.FForward.TabIndex = 4;
            this.FForward.Text = ">>";
            this.FForward.Click += new System.EventHandler(this.FForward_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(447, 128);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(120, 311);
            this.textBox1.TabIndex = 6;
            this.textBox1.Text = "please oepn a .sgf file to view, or just play on the board";
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // GoBoard
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(581, 478);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.Rewind);
            this.Controls.Add(this.FForward);
            this.Controls.Add(this.Save);
            this.Controls.Add(this.Open);
            this.Controls.Add(this.Back);
            this.Controls.Add(this.Forward);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "GoBoard";
            this.ShowIcon = false;
            this.Text = "华夏围棋";
            this.Click += new System.EventHandler(this.GoBoard_Click);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.PaintHandler);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUpHandler);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

        //未实现的功能。但要在properties中做出相应修正（即修正调用者）
		protected void textBox1_TextChanged (object sender, System.EventArgs e)
		{
			return;
		}

		private void PaintHandler(Object sender, PaintEventArgs e)
		{
			UpdateGoBoard(e);
		}

        //单击保存，未实现
		protected void SaveClick (object sender, System.EventArgs e)
		{
			return;
		}

		protected void Open_Click (object sender, System.EventArgs e)
		{
			OpenFile();
			showGameInfo();
		}

		protected void Rewind_Click (object sender, System.EventArgs e)
		{
			gameTree.reset();
			resetBoard();
            showGameInfo();
		}

		protected void FForward_Click (object sender, System.EventArgs e)
		{
            if (gameTree != null)
            {
                int i = 0; 
                GoMove gm = null;
                for (gm = gameTree.doNext(); gm != null; gm = gameTree.doNext()) 
                {
                    playNext(ref gm);
                    if (i++ > nFFMove)
                        break; 
                }
            }
		}

		protected void Forward_Click (object sender, System.EventArgs e)
		{
			GoMove gm = gameTree.doNext();
			if (null != gm)
			{
				playNext(ref gm);
			}
		}

		private void showGameInfo()
		{
			//display game infomation in textbox
			textBox1.Clear();
			textBox1.AppendText(gameTree.Info);
		}

		protected void Back_Click (object sender, System.EventArgs e)
		{
			GoMove gm = gameTree.doPrev();	//move back to prev. situation
            if (null != gm)
            {
                playPrev(gm);
            }
            else
            {
                resetBoard();
                showGameInfo(); 
            }
		}

		Boolean onBoard(int x, int y) 
		{
			return (x>=0 && x<nSize && y>=0 && y<nSize);
		}

		protected void GoBoard_Click (object sender, System.EventArgs e)
		{
			return;
		}

		private Point PointToGrid(int x, int y)
		{
			Point p= new Point(0,0);
			p.X = (x - rGrid.X + nUnitGridWidth/2) / nUnitGridWidth;
			p.Y = (y - rGrid.Y + nUnitGridWidth/2) / nUnitGridWidth;
			return p;
		}

		//tell where the user wanted to place the stone
		//stone will place at the cross node if (x, y) is within 1/3 grid width
		private Boolean closeEnough(Point p, int x, int y)
		{
			if (x < rGrid.X+nUnitGridWidth*p.X-nUnitGridWidth/3 ||
				x > rGrid.X+nUnitGridWidth*p.X+nUnitGridWidth/3 ||
				y < rGrid.Y+nUnitGridWidth*p.Y-nUnitGridWidth/3 ||
				y > rGrid.Y+nUnitGridWidth*p.Y+nUnitGridWidth/3)
			{
				return false;
			}
			else 
				return true;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <ZZZZZ ZZZZ="ZZZZZZ"></ZZZZZ>
        /// <ZZZZZ ZZZZ="Z"></ZZZZZ>
		private void MouseUpHandler(Object sender,MouseEventArgs e)
		{
			Point p;
			GoMove	gmThisMove;

			p = PointToGrid(e.X,e.Y);
			if (!onBoard(p.X, p.Y) || !closeEnough(p,e.X, e.Y)|| Grid[p.X,p.Y].hasStone())
				return; //this click will not place a stone

			//stone placed and move forward to next player
			gmThisMove = new GoMove(p.X, p.Y, m_colorToPlay, 0);
			playNext(ref gmThisMove);
			gameTree.addMove(gmThisMove);
		}

		public void playNext(ref GoMove gm) 
		{
			Point p = gm.Point;
			m_colorToPlay = gm.Color;	//change color to next player

			//change the label spot on the last placed stone
			clearLabelsAndMarksOnBoard(); 
			
			if (m_gmLastMove != null)
				repaintOneSpotNow(m_gmLastMove.Point);

			bDrawMark = true;
			Grid[p.X,p.Y].setStone(gm.Color);
			m_gmLastMove = new GoMove(p.X, p.Y, gm.Color, nSeq++);
			//mark and label the last placed stone
			setLabelsOnBoard(gm);
			setMarksOnBoard(gm);
			
			doDeadGroup(nextTurn(m_colorToPlay));
			//if any stone will be killed
			if (m_fAnyKill)
				appendDeadGroup(ref gm, nextTurn(m_colorToPlay));//！！代码风格问题，if后面的{}不可以省略，便于维护
			else
			{
				doDeadGroup(m_colorToPlay);
				if (m_fAnyKill)
                    appendDeadGroup(ref gm, m_colorToPlay);//！！代码风格问题，if后面的{}不可以省略，便于维护
			}
			m_fAnyKill = false;
			
			optRepaint();

			//change next stone color
			m_colorToPlay = nextTurn(m_colorToPlay);
			
			//update information on the textbox
			textBox1.Clear();
			textBox1.AppendText(gm.Comment);
		}

		private void appendDeadGroup(ref GoMove gm, StoneColor c)
		{
			ArrayList a = new ArrayList();
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
					if (Grid[i,j].isKilled())
					{
						Point pt = new Point(i,j);
						a.Add(pt);
						Grid[i,j].setNoKilled();
					}
			gm.DeadGroup = a;
			gm.DeadGroupColor = c;
		}

		public void resetBoard()
		{
			int i,j;
			for (i=0; i<nSize; i++)
				for (j=0; j<nSize; j++) 
					Grid[i,j].removeStone();
			m_gmLastMove = null;
			Invalidate(null);
		}

		/*
		 * play the move so that situation reverted to before this move
		 * to do:
		 * 	1. remove current move
		 *  1.1 remove its highlight
		 *	2. store the stones got killed by current move
		 *  3. update the highlight
		 */
		public void playPrev(GoMove gm)
		{
            Point p = gm.Point;
            m_colorToPlay = gm.Color;  

            clearLabelsAndMarksOnBoard();
            m_gmLastMove = gameTree.peekPrev();

            bDrawMark = true;
            Grid[p.X, p.Y].die();
            if (gm.DeadGroup != null)
            {
                foreach (Point pt in gm.DeadGroup)
                {
                    Grid[pt.X, pt.Y].setStone(gm.DeadGroupColor);
                }
            }
            optRepaint();

            textBox1.Clear();
            textBox1.AppendText(gm.Comment);

            return; 
        }

				
		
		Rectangle getUpdatedArea(int i, int j) 
		{
			int x = rGrid.X + i * nUnitGridWidth - nUnitGridWidth/2;
			int y = rGrid.Y + j * nUnitGridWidth - nUnitGridWidth/2;
			return new Rectangle(x,y, nUnitGridWidth, nUnitGridWidth);
		}

		/**
		 * repaint each grid in board
		 */
		private void optRepaint()
		{
			Rectangle r = new Rectangle(0,0,0,0);
			Region	re;

			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
					if (Grid[i,j].isUpdated()) 
					{
						r = getUpdatedArea(i,j);
						re = new Region(r);
						Invalidate(re);
					}
		}

		/*
		 * repaint part of the board
		 */
		void repaintOneSpotNow(Point p)
		{
			Grid[p.X, p.Y].setUpdated();
			bDrawMark = false;
			Rectangle r = getUpdatedArea(p.X, p.Y);
			Invalidate( new Region(r));
			Grid[p.X, p.Y].resetUpdated();
			bDrawMark = true;
		}

		//无调用的函数，删掉或调用
// 		void recordMove(Point p, StoneColor colorToPlay) 
// 		{
// 			Grid[p.X,p.Y].setStone(colorToPlay);
// 			// save location, color and sequence number
// 			m_gmLastMove = new GoMove(p.X, p.Y, colorToPlay, nSeq++);
// 		}

		StoneColor nextTurn(StoneColor c) 
		{
			if (c == StoneColor.black)
				return StoneColor.white;
			else 
				return StoneColor.black;
		}

		/**
		 *	bury the dead stones in a group (same color). 
		 *	if a stone in one group is dead, the whole group is dead.
		*/
		void buryTheDead(int i, int j, StoneColor c) 
		{
			if (onBoard(i,j) && Grid[i,j].hasStone() && 
				Grid[i,j].color() == c) 
			{
				Grid[i,j].die();
				buryTheDead(i-1, j, c);
				buryTheDead(i+1, j, c);
				buryTheDead(i, j-1, c);
				buryTheDead(i, j+1, c);
			}
		}

		void cleanScanStatus()
		{
			int i,j;
			for (i=0; i<nSize; i++)
				for (j=0; j<nSize; j++) 
					Grid[i,j].clearScanned();
		}

		/**
		 * ZZZZZZ ZZZ ZZZZ ZZZZZ ZZZ ZZZZZ ZZZZ ZZZZ ZZZ ZZZZZ.
		 */
		void doDeadGroup(StoneColor c) 
		{
			int i,j;
			for (i=0; i<nSize; i++)
				for (j=0; j<nSize; j++) 
					if (Grid[i,j].hasStone() &&
						Grid[i,j].color() == c) 
					{
						if (calcLiberty(i,j,c) == 0)
						{
							buryTheDead(i,j,c);
							m_fAnyKill = true;
						}
						cleanScanStatus();
					}
		}


		/**
		 * if liberty is zero, a stone is dead or stone group is dead
		 */
		int calcLiberty(int x, int y, StoneColor c) 
		{
			int lib = 0; // initial to 0, and add liberty in follow steps
			
			if (!onBoard(x,y))
				return 0;
			if (Grid[x,y].isScanned())
				return 0;

			if (Grid[x,y].hasStone()) 
			{
				if (Grid[x,y].color() == c) 
				{
					//neighbor stone shares the same color
					Grid[x,y].setScanned();
					lib += calcLiberty(x-1, y, c);
					lib += calcLiberty(x+1, y, c);
					lib += calcLiberty(x, y-1, c);
					lib += calcLiberty(x, y+1, c);
				} 
				else 
					return 0;
			} 
			else 
			{// neighbor to empty
				lib ++;
				Grid[x,y].setScanned();
			}

			return lib;
		}


		/**
		 * mark last placed stone
		 */
		void markLastMove(Graphics g) 
		{
			Brush brMark;
			if (m_gmLastMove.Color == StoneColor.white)
				brMark = brBlack;
			else 
				brMark = brWhite;
			Point p = m_gmLastMove.Point;
			g.FillRectangle( brMark,
				rGrid.X + (p.X) * nUnitGridWidth - (nUnitGridWidth-1)/8, 
				rGrid.Y + (p.Y) * nUnitGridWidth - (nUnitGridWidth-1)/8,
				3, 3);
		}

		private void clearLabelsAndMarksOnBoard()
		{
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
				{
					if (Grid[i,j].hasLabel())
						Grid[i,j].resetLabel();
					if (Grid[i,j].hasMark())
						Grid[i,j].resetMark();
				}

		}

		private void setLabelsOnBoard(GoMove gm)
		{
			short	nLabel = 0;
			Point p;
			if (null != gm.Labels)
			{
                // i未使用
			    //int i = gm.Labels.Count;
				//i = gm.Labels.Capacity;

				System.Collections.IEnumerator myEnumerator = gm.Labels.GetEnumerator();
				while (myEnumerator.MoveNext())
				{
					p = (Point)myEnumerator.Current;
					Grid[p.X,p.Y].setLabel(++nLabel);
				}
			}
		}

		private void setMarksOnBoard(GoMove gm)
		{
			Point p;
			if (null != gm.Labels)
			{
				System.Collections.IEnumerator myEnumerator = gm.Marks.GetEnumerator();
				while ( myEnumerator.MoveNext() )
				{
					p = (Point)myEnumerator.Current;
					Grid[p.X,p.Y].setMark();
				}
			}
		}

		private Point SwapXY(Point p)
		{
			Point pNew = new Point(p.Y,p.X);
			return pNew;
		}

		private void DrawBoard(Graphics g)
		{
			//stone on the board has v and h from 1 to 19
			string[] strV= {"1","2","3","4","5","6","7","8","9","10","11","12","13","14","15","16","17","18","19"};
			string [] strH= {"A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Q","R","S","T"};

			Point p1 = new Point(nEdgeLen,nEdgeLen);
			Point p2 = new Point(nTotalGridWidth+nEdgeLen,nEdgeLen);
			g.FillRectangle(brBoard,nBoardOffset,nBoardOffset,nTotalGridWidth+nBoardOffset,nTotalGridWidth+nBoardOffset);
			for (int i=0;i<nSize; i++)
			{
				g.DrawString(strV[i],this.Font, brBlack, 0, nCoodStart+ nBoardOffset + nUnitGridWidth*i);
				g.DrawString(strH[i],this.Font, brBlack, nBoardOffset + nCoodStart + nUnitGridWidth*i, 0);
				g.DrawLine(penGrid, p1, p2);
				g.DrawLine(penGrid, SwapXY(p1), SwapXY(p2));

				p1.Y += nUnitGridWidth;
				p2.Y += nUnitGridWidth;
			}
			
			Pen penHi = new Pen(Color.WhiteSmoke, (float)0.5);
			Pen penLow = new Pen(Color.Gray, (float)0.5);

			g.DrawLine(penHi, nBoardOffset, nBoardOffset, nTotalGridWidth+2*nBoardOffset, nBoardOffset);
			g.DrawLine(penHi, nBoardOffset, nBoardOffset, nBoardOffset, nTotalGridWidth+2*nBoardOffset);
			g.DrawLine(penLow, nTotalGridWidth+2*nBoardOffset,nTotalGridWidth+2*nBoardOffset, nBoardOffset+1, nTotalGridWidth+2*nBoardOffset);
			g.DrawLine(penLow, nTotalGridWidth+2*nBoardOffset,nTotalGridWidth+2*nBoardOffset, nTotalGridWidth+2*nBoardOffset, nBoardOffset+1);
		}

		void UpdateGoBoard(PaintEventArgs e)
		{
			DrawBoard(e.Graphics);

			drawStars(e.Graphics);

			//draw every spot on board
			drawEverySpot(e.Graphics);
		}

		//star is the bold points, which is special cross nodes
		void drawStar(Graphics g, int row, int col) 
		{
			g.FillRectangle(brStar,
				rGrid.X + (row-1) * nUnitGridWidth - 1, 
				rGrid.Y + (col-1) * nUnitGridWidth - 1, 
				3, 
				3);
		}

		//for a 19*19 board, there are follow stars on board
		void  drawStars(Graphics g)
		{
			drawStar(g, 4, 4);
			drawStar(g, 4, 10);
			drawStar(g, 4, 16);
			drawStar(g, 10, 4);
			drawStar(g, 10, 10);
			drawStar(g, 10, 16);
			drawStar(g, 16, 4);
			drawStar(g, 16, 10);
			drawStar(g, 16, 16);
		}

		/**
		 * draw the color and place of a stone
		 */
		void drawStone(Graphics g, int row, int col, StoneColor c) 
		{
			Brush br;
			if (c == StoneColor.white)
				br = brWhite;
			else 
				br = brBlack;
			
			Rectangle r = new Rectangle(rGrid.X+ (row) * nUnitGridWidth - (nUnitGridWidth-1)/2, 
				rGrid.Y + (col) * nUnitGridWidth - (nUnitGridWidth-1)/2,
				nUnitGridWidth-1,
				nUnitGridWidth-1);

			g.FillEllipse(br, r);
		}

		void drawLabel(Graphics g, int x, int y, short nLabel) 
		{
			if (nLabel ==0)
				return;
			nLabel --;
			nLabel %= 18;			//?

			Rectangle r = new Rectangle(rGrid.X+ x * nUnitGridWidth - (nUnitGridWidth-1)/2, 
				rGrid.Y + y * nUnitGridWidth - (nUnitGridWidth-1)/2,
				nUnitGridWidth-1,
				nUnitGridWidth-1);

			g.FillEllipse(brBoard, r);

			g.DrawString(strLabels[nLabel],	//size of the string is 1/2 of gird size
				this.Font, 
				brBlack, 
				rGrid.X+ (x) * nUnitGridWidth - (nUnitGridWidth-1)/4, 
				rGrid.Y + (y) * nUnitGridWidth - (nUnitGridWidth-1)/2);
		}

		void drawMark(Graphics g, int x, int y)
		{
			g.FillRectangle( m_brMark,
				rGrid.X + x* nUnitGridWidth - (nUnitGridWidth-1)/8, 
				rGrid.Y + y * nUnitGridWidth - (nUnitGridWidth-1)/8,
				5, 5);
		}

		void drawEverySpot(Graphics g) 
		{
			for (int i=0; i<nSize; i++)
				for (int j=0; j<nSize; j++)
				{
					if (Grid[i,j].hasStone())
						drawStone(g, i, j, Grid[i,j].color());
					if (Grid[i,j].hasLabel())
						drawLabel(g, i, j, Grid[i,j].getLabel());
					if (Grid[i,j].hasMark())
						drawMark(g, i, j);
				}
			//new stone placed
			if (bDrawMark && m_gmLastMove != null)
				markLastMove(g);
		}

		//open sgf and draw
		private void OpenFile()
		{
			OpenFileDialog openDlg = new OpenFileDialog();
			openDlg.Filter  = "sgf files (*.sgf)|*.sgf|All Files (*.*)|*.*";
			openDlg.FileName = "" ;
			openDlg.DefaultExt = ".sgf";
			openDlg.CheckFileExists = true;
			openDlg.CheckPathExists = true;
			
			DialogResult res = openDlg.ShowDialog ();
			
			if(res == DialogResult.OK)
			{
				if( !(openDlg.FileName).EndsWith(".sgf") && !(openDlg.FileName).EndsWith(".SGF")) 
					MessageBox.Show("Unexpected file format","Super Go Format",MessageBoxButtons.OK);
				else
				{
					FileStream f = new FileStream(openDlg.FileName, FileMode.Open); 
					StreamReader r = new StreamReader(f);
					string s = r.ReadToEnd();
					gameTree = new GoTree(s);
					gameTree.reset();
                    resetBoard();
					r.Close(); 
					f.Close();
				}
			}		
		}	
	}

	public static class GoTest // 只含有静态变量，可以声明为静态类，以优化编译
	{
		/// <summary>
		/// entry
		/// </summary>
        [STAThread]
		public static void Main(string[] args) 
		{
			Application.Run(new GoBoard(19));
		}
	}

	
	//spot is the cross of lines
	public class Spot 
	{
		private Boolean bEmpty;
		private Boolean bKilled;
		private Stone s;
		private short	m_nLabel;
		private Boolean m_bMark;
		private Boolean bScanned;
		private Boolean bUpdated;
		/**
		 * states of spot is as follows
		 */
		public Spot() 
		{
			bEmpty = true;
			bScanned = false;
			bUpdated = false;
			bKilled = false;
		}
		
		public Boolean hasStone() { return !bEmpty;	}
		public Boolean isEmpty() {	return bEmpty;	}
		public Stone thisStone() {	return s;}
		public StoneColor color() {	return s.color;}

		public Boolean hasLabel() {return m_nLabel>0;}
		public Boolean hasMark() {return m_bMark;}
		public void setLabel(short l) {m_nLabel = l; bUpdated = true; }
		public void setMark() {m_bMark = true; bUpdated = true;}
		public void resetLabel() {m_nLabel = 0; bUpdated = true;}
		public void resetMark() {m_bMark = false; bUpdated = true;}
		public short	getLabel() {return m_nLabel;}

		public Boolean isScanned() { return bScanned;}
		public void setScanned() {	bScanned = true;}
		public void clearScanned() { bScanned = false; }

		public void setStone(StoneColor c) 
		{
			if (bEmpty) 
			{
				bEmpty = false;
				s.color = c;
				bUpdated = true;
			} // stone states
		}

		/*
		 * remove and set state
		*/
		public void removeStone()
		{	
			bEmpty = true;
			bUpdated = true;
		}
				
		//change of state when die happens
		public void die() 
		{
			bKilled = true;
			bEmpty = true;
			bUpdated = true;
		} 

		public Boolean isKilled() { return bKilled;}
		public void setNoKilled() { bKilled = false;}

		public void resetUpdated() { bUpdated = false; bKilled = false;}


		public Boolean isUpdated() 
		{ 
			if (bUpdated)
			{	//updated state
				bUpdated = false;
				return true;
			} 
			else 
				return false;
		}


		public void setUpdated() { bUpdated = true; }
	}

	/**
	 * Z ZZZZ ZZ Z ZZ ZZZZ.
	 */
	public class GoMove 
	{
		StoneColor m_c;	
		Point m_pos;		
		int m_n;			
		String m_comment;	
		MoveResult m_mr;	

		ArrayList		m_alLabel; 
		ArrayList		m_alMark; 


		ArrayList		m_alDead;
		StoneColor	m_cDead;
		/**
		 * one move has following attributes
		 */
		public GoMove(int x, int y, StoneColor sc, int seq) 
		{
			m_pos = new Point(x,y);
			m_c = sc;
			m_n = seq;
			m_mr = new MoveResult();
			m_alLabel = new ArrayList();
			m_alMark = new ArrayList();
		}

		public GoMove(String str, StoneColor c) 
		{
			char cx = str[0];
			char cy = str[1];
			m_pos = new Point(0,0);
			//ZZZ Z# ZZ ZZZ ZZZZZZZZZ - 
			m_pos.X = (int) ( (int)cx - (int)(char)'a');
			m_pos.Y = (int) ( (int)cy - (int)(char)'a');
			this.m_c = c;
			m_alLabel = new ArrayList();
			m_alMark = new ArrayList();
		}


		private Point	StrToPoint(String str)
		{
			Point p = new Point(0,0);
			char cx = str[0];
			char cy = str[1];
			//ZZZ Z# ZZ ZZZ ZZZZZZZZZ - 
			p.X = (int) ( (int)cx - (int)(char)'a');
			p.Y = (int) ( (int)cy - (int)(char)'a');
			return p;
		}


        public StoneColor Color
        { 
            get { return m_c; } 
        }

        public String Comment 
        {
            get
            {
                if (m_comment == null)
                    return string.Empty;
                else
                    return m_comment;
            }
            set
            {
                m_comment = value; 
            }
        }

		public int Seq
        {
            get { return m_n; }
            set {	m_n = value;}
        }

        public Point Point
        {
           get  { return m_pos; }
        }

        public MoveResult Result
        {
            get { return m_mr; }
            set { m_mr = value; }
        }
		
		public ArrayList DeadGroup
        {
            get { return m_alDead;}
            set {m_alDead = value;}
        }

        public StoneColor DeadGroupColor
        {
            get { return m_cDead; }
            set { m_cDead = value; }
        }
		
		public void addLabel(String str) {m_alLabel.Add(StrToPoint(str));}
		
		public void addMark(String str) {	m_alMark.Add(StrToPoint(str));}

        public ArrayList Labels
        {
            get { return m_alLabel; }
        }

        public ArrayList Marks
        {
            get { return m_alMark; }
        }
	}
	

	/**
	 * after a move variables should be changed
	 * 
	 */
	public class MoveResult 
	{
		public StoneColor color;  //这里最好声明成private，但是因为只是个游戏，就不改了
		// 4 attribute of a move 
		public Boolean bUpKilled;
		public Boolean bDownKilled;
		public Boolean bLeftKilled;
		public Boolean bRightKilled;
		public Boolean bSuicide;	
		public MoveResult() 
		{
			bUpKilled = false;
			bDownKilled = false;
			bLeftKilled = false;
			bRightKilled = false;
			bSuicide = false;
		}
	}

	/**
	 * a stone has a color
	 */
	public struct Stone 
	{
		public StoneColor color; 
	}


	public class GoVariation 
	{
		int m_id;
		string m_name;
		ArrayList m_moves; 
		int m_seq;			 
		int m_total;

		public GoVariation(int id)
		{
			m_id = id;
			m_moves = new ArrayList(10);
			m_seq = 0;
			m_total = 0;
		}

		public void addAMove(GoMove gm) 
		{
			gm.Seq = m_total ++;
			m_seq++;
			m_moves.Add(gm);
		}

		public void updateResult(GoMove gm) 
		{
		}

		public GoMove doNext()
		{
			if (m_seq < m_total) 
			{
				return (GoMove)m_moves[m_seq++];
			} 
			else 
				return null;
		}

		public GoMove doPrev()
		{
			if (m_seq > 0)
				return (GoMove)(m_moves[--m_seq]);
			else 
				return null;
		}

		/*
		 *  prev move of other player
		 */
		public GoMove peekPrev()
		{
			if (m_seq > 0)
				return (GoMove)(m_moves[m_seq-1]);
			else 
				return null;
		}

		public void reset() {m_seq = 0;}
	}


// 	struct VarStartPoint
// 	{
// 		int m_id; 
// 		int m_seq;
// 	}

	struct GameInfo 
	{
		public string gameName;
		public string playerBlack;
		public string playerWhite;
		public string rankBlack;
		public string rankWhite;
		public string result;
		public string date;
		public string km;
		public string size;
		public string comment;
        public string handicap;
        public string gameEvent;
        public string location;
        public string time;             
        public string unknown_ff;   
        public string unknown_gm;
        public string unknown_vw; 
	}

	public class KeyValuePair 
	{
		public string k; 
        public ArrayList alV; //每行只定义一个变量，有利于设定断点，读代码的时候也可以有目的的读不必为了找一处而读整行

		private string	removeBackSlash(string strIn)
		{
			string strOut; 
			int		iSlash;

			strOut = string.Copy(strIn);
			if (strOut.Length < 2)
				return strOut;
			for (iSlash = strOut.Length-2; iSlash>=0; iSlash--)
			{
				if (strOut[iSlash] == '\\')	
				{
					strOut = strOut.Remove(iSlash,1);
					if (iSlash>0)
						iSlash --;	
				}
			}
			return strOut;
		}

		public KeyValuePair(string k, string v)
		{
			this.k = string.Copy(k);
			string strOneVal;
			int		iBegin, iEnd;

			alV = new ArrayList(1);

			if (k.Equals("C"))
			{
				strOneVal = removeBackSlash(string.Copy(v));
				alV.Add(strOneVal);
				return;
			}

			iBegin = v.IndexOf("[");
			if (iBegin == -1)	
			{
				alV.Add(v);
				return; 
			}
			
			iBegin = 0;
			while (iBegin < v.Length && iBegin>=0)
			{
				iEnd = v.IndexOf("]", iBegin);
				if (iEnd > 0)
					strOneVal = v.Substring(iBegin, iEnd-iBegin);
				else 
					strOneVal = v.Substring(iBegin);	
				alV.Add(strOneVal);
				iBegin = v.IndexOf("[", iBegin+1);
				if (iBegin > 0)
					iBegin ++;	
			}
		}
	}


	public class GoTree 
	{
		GameInfo _gi;		//ZZZZZ ZZZ ZZZZ'Z ZZZZZZZ ZZZZ.
		ArrayList _vars;		//ZZZZZZZZZZ. 
		int _currVarId;		//ZZ ZZ ZZZZZZZ ZZZZZZZZZ.
		int _currVarNum;
		GoVariation _currVar;		//ZZZZZZZ ZZZZZZZZZZZ.
		string	_stGameComment;

		// ZZZZZZZZZZZ - ZZZZZZ ZZZ ZZZZZZ ZZZZZ ZZ ZZZZZ ZZZZZZ
		public GoTree(string s)
		{
			_vars = new ArrayList(10);
			_currVarNum = 0;
			_currVarId = 0; 
			_currVar = new GoVariation(_currVarId);
			_vars.Add(_currVar);
			parseFile(s);
		}

		//	ZZZZZZZZZZZ - ZZZZZZ ZZ ZZZZZ ZZZZZZ
		public GoTree()
		{
			_vars = new ArrayList(10);
			_currVarNum = 0;
			_currVarId = 0; 
			_currVar = new GoVariation(_currVarId);
			_vars.Add(_currVar);
		}

		public	string Info
		{
            get
            {
                return _gi.comment == null? string.Empty : _gi.comment;
            }
		}

		public void addMove(GoMove gm) 
		{
			_currVar.addAMove(gm);
		}

		/**
		 * 下面的都是文件操作 
		 */
		Boolean parseFile(String goStr) 
		{
			int iBeg, iEnd=0; 
			while (iEnd < goStr.Length) 
			{
				if (iEnd > 0)
					iBeg = iEnd;
				else 
					iBeg = goStr.IndexOf(";", iEnd);
				iEnd = goStr.IndexOf(";", iBeg+1);
				if (iEnd < 0) //ZZ ZZZZ ";"
					iEnd = goStr.LastIndexOf(")", goStr.Length);		
				if (iBeg >= 0 && iEnd > iBeg) 
				{
					string section = goStr.Substring(iBeg+1, iEnd-iBeg-1);
					parseASection(section);
				} 
				else 
					break;
			}
			return true;
		}

        int findEndofValueStr(String sec)
        {
            int i = 0;
            while (i >= 0)
            {
                i = sec.IndexOf(']', i+1);
                if (i > 0 && sec[i - 1] != '\\')
                    return i;  
            }

            return sec.Length - 1;	
        }
        
//         int findEndofValueStrOld(String sec)
// 		{
// 			int i = 0;
//             //ZZ ZZZZZZ ZZ'ZZ ZZZZZZZZ ZZZZ ZZZZZZZ ZZZ ZZZZZ ZZZZZZ. 
// 			bool fOutside = false;
// 			
// 			for (i=0; i<sec.Length;i++)
// 			{
// 				if (sec[i] == ']')
// 				{
// 					if (i>1 && sec[i-1] != '\\') 
// 						fOutside = true;
// 				}
// 				else if (char.IsLetter(sec[i]) && fOutside && i>0)
// 					return i-1;
// 				else if (fOutside && sec[i] == '[')
// 					fOutside = false;
// 			}
// 			return sec.Length-1;
// 		}

        private string purgeCRLFSuffix(string inStr)
        {
            int iLast = inStr.Length - 1; 

            if (iLast <= 0)
                return inStr; 

            while ((inStr[iLast] == '\r' || inStr[iLast] == '\n' || inStr[iLast] == ' '))
            {
                iLast--; 
            }
            if ((iLast+1) != inStr.Length)
                return inStr.Substring(0, iLast+1);  
            else
                return inStr; 
        }
 
		Boolean parseASection(String sec) 
		{
			int iKey = 0;
			int iValue = 0;
			int iLastValue = 0;
			KeyValuePair kv;
			ArrayList Section = new ArrayList(10);
			
			try 
			{
				iKey = sec.IndexOf("[");
				if (iKey < 0)
				{
					return false;
				}
                sec = purgeCRLFSuffix(sec);
 
				iValue = findEndofValueStr(sec); 
				iLastValue = sec.LastIndexOf("]");
				if (iValue <= 0 || iLastValue <= 1)
				{
					return false;
				}
				sec = sec.Substring(0,iLastValue+1);
				while (iKey > 0 && iValue > iKey)
				{
					string key = sec.Substring(0,iKey);
					int iNonLetter = 0;
					while (!char.IsLetter(key[iNonLetter]) && iNonLetter < key.Length)
						iNonLetter ++;
					key = key.Substring(iNonLetter);

					string strValue = sec.Substring(iKey+1, iValue-iKey-1);
				    //kv is key value contains key and string
					kv = new KeyValuePair(key, strValue);
					Section.Add(kv);
					if (iValue >= sec.Length)
						break;
					sec = sec.Substring(iValue+1);
					iKey = sec.IndexOf("[");
					if (iKey > 0)
					{
						iValue = findEndofValueStr(sec);
					}
				}
			}
			catch
			{
                return false;
            }

			processASection(Section);
			return true;
		}

        Boolean processASection(ArrayList arrKV) 
		{
			Boolean fMultipleMoves = false;   // mutiple move
			GoMove gm = null; 
            
			string key, strValue;

			for (int i = 0;i<arrKV.Count;i++)
			{
				key = ((KeyValuePair)(arrKV[i])).k;
				for (int j=0; j<((KeyValuePair)(arrKV[i])).alV.Count; j++)
				{
					strValue = (string)(((KeyValuePair)(arrKV[i])).alV[j]);

                    //----------------------------------------------------------------------------------
                    //下面的if else太复杂 写成case语句较好
                    switch(key)
                    {
                        case "B":
                            Debug.Assert(gm == null);
                            gm = new GoMove(strValue, StoneColor.black);
                            break;
                        case "W":
                            Debug.Assert(gm == null);
                            gm = new GoMove(strValue, StoneColor.white);
                            break;
                        case "C":
                            if (gm != null)
                                gm.Comment = strValue;
                            else	
                                _gi.comment += strValue;
                            break;
                        case "L":
                            if (gm != null)
                                gm.addLabel(strValue);
                            else	
                                _stGameComment += strValue;
                            break;
                        case "M":
                            if (gm != null)
                                gm.addMark(strValue);
                            else	//ZZZ ZZ ZZ ZZZ ZZZZ ZZZZZZZ 
                                _gi.comment += strValue;
                            break;
                        case "AW":
                            fMultipleMoves = true;
                            gm = new GoMove(strValue, StoneColor.white);
                            break;
                        case "AB":
                            fMultipleMoves = true;
                            gm = new GoMove(strValue, StoneColor.black);
                            break;
                        case "HA":
                            _gi.handicap = (strValue);
                            break;
                        case "BS":
                        case "WS":
                        case "ID":
                        case "KI":
                        case "SO":
                        case "TR":
                        case "LB":
                        case "RO":
                             _gi.unknown_vw = (strValue);
                            break;
                        default:
                            if (key.Equals("BR"))
                                _gi.rankBlack = (strValue);
                            else if (key.Equals("PB"))
                                _gi.playerBlack = (strValue);
                            else if (key.Equals("PW"))
                                _gi.playerWhite = (strValue);
                            else if (key.Equals("WR"))
                                _gi.rankWhite = (strValue);
                            else if (key.Equals("DT"))
                                _gi.date = (strValue);
                            else if (key.Equals("KM"))
                                _gi.km = (strValue);
                            else if (key.Equals("RE"))
                                _gi.result = (strValue);
                            else if (key.Equals("SZ"))
                                _gi.size = (strValue);
                            else if (key.Equals("EV"))
                                _gi.gameEvent = (strValue);
                            else if (key.Equals("PC"))
                                _gi.location = (strValue);
                            else if (key.Equals("TM"))
                                _gi.time = (strValue);
                            else if (key.Equals("GN"))
                                _gi.gameName = strValue;
                            else if (key.Equals("FF"))
                                _gi.unknown_ff = (strValue);
                            else if (key.Equals("GM"))
                                _gi.unknown_gm = (strValue);
                            else if (key.Equals("VW"))
                                _gi.unknown_vw = (strValue);
                            else if (key.Equals("US"))
                                _gi.unknown_vw = (strValue);
                            else
                                System.Diagnostics.Debug.Assert(false, "unhandle key: " + key + " " + strValue);
                            break;                                              
                    }
                    if (fMultipleMoves)
                    {
                        _currVar.addAMove(gm);
                    }
                }
            }
            if (!fMultipleMoves && gm != null)
            {
                _currVar.addAMove(gm);
            }
			return true;
		} 

		public GoMove doPrev() 
		{
			return _currVar.doPrev();
		}

		public GoMove peekPrev() 
		{
			return _currVar.peekPrev();
		}

		public GoMove doNext() 
		{
			return _currVar.doNext();
		}

		public void updateResult(GoMove gm) 
		{
			_currVar.updateResult(gm);
		}
		
		public void reset()
		{
			_currVarNum = 0;
			_currVarId = 0; 
			_currVar.reset();
		}
		public void rewindToStart()
		{

		}
	} //end go tree
}
