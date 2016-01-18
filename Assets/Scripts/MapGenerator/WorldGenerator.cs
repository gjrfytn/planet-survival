using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class RiversParameters
{
	public float Height; //Коэффициент высоты реки относительно средней высоты
	public byte Count;
	public ushort MinimumLength;
	public byte Attempts; //Количество попыток построения реки из одного хекса
	public float FlowHeightKoef; //Насколько реалистично река распространяется относительно высоты (1 - самое реалистичное)
}

public class WorldGenerator : MonoBehaviour
{
	//public ushort Width; //TODO Переменные размеры
	//public ushort Height;
	
	public float LandscapeRoughness;
	public float ForestRoughness;
	public RiversParameters RiversParam;

	//TODO Проверить использование координат в RiverStack
	private Stack<Vector2> RiverStack = new Stack<Vector2> (); //Стек для постройки реки

	/// <summary>
	/// Создаёт карту высот. 
	/// </summary>
	/// <param name="matrix">[out] Карта высот.</param>
	/// <param name="r">Шероховатость.</param>
	/// Использует фрактальный алгоритм Diamond Square
	public void CreateHeightmap (float[,] matrix, float r, float topLeft,float topRight,float bottomLeft,float bottomRight)
	{
		ushort height = (ushort)matrix.GetLength (0);
		ushort width = (ushort)matrix.GetLength (1);


		//Задаём начальные значения по углам
		matrix [0, 0] = topLeft; 
		matrix [0, width - 1] = topRight;
		matrix [height - 1, 0] = bottomLeft;
		matrix [height - 1, width - 1] =bottomRight;

		for (ushort stepY=(ushort)(height-1),stepX=(ushort)(width-1),halfY=(ushort)(stepY/2),halfX=(ushort)(stepX/2); halfY!=0; stepY/=2, halfY/=2,stepX/=2, halfX/=2)
		{
			float randRange=((halfY+halfX)/2f)/((height+width)/2f);

			for (ushort y=halfY; y<height; y+=stepY)
				for (ushort x=halfX; x<width; x+=stepX)
					matrix [y, x] = (matrix [y - halfY, x - halfX] + matrix [y + halfY, x - halfX] + matrix [y - halfY, x + halfX] + matrix [y + halfY, x + halfX]) / 4 + (Random.Range (-randRange * r, randRange * r));

			for (ushort y=halfY; y<height; y+=stepY) 
			{
				matrix [y, 0] = (matrix [y - halfY, 0] + 0 + matrix [y + halfY, 0] + matrix [y, 0 + halfX]) / 4 + (Random.Range (-randRange * r, randRange * r));
				matrix [y, width - 1] = (matrix [y - halfY, width - 1] + matrix [y, width  - 1 - halfX] + matrix [y + halfY, width - 1] + 0) / 4 + (Random.Range (-randRange * r, randRange * r));
			}

			for (ushort x=halfX; x<width; x+=stepX) 
			{
				matrix [0, x] = (0 + matrix [0, x - halfX] + matrix [0 + halfY, x] + matrix [0, x + halfX]) / 4 + (Random.Range (-randRange * r, randRange * r));
				matrix [height - 1, x] = (matrix [height - 1 - halfY, x] + matrix [height - 1, x - halfX] + 0 + matrix [height - 1, x + halfX]) / 4 + (Random.Range (-randRange * r, randRange * r));
			}

			for(ushort y=halfY;y<height-1;y+=stepY)
				for(ushort x=stepX;x<width-1;x+=stepX)
					matrix[y,x]=(matrix [y, x - halfX] + matrix [y - halfY, x] + matrix [y, x + halfX] + matrix [y + halfY, x]) / 4 + (Random.Range (-randRange * r, randRange * r));

			for(ushort x=halfX;x<width-1;x+=stepX)
				for(ushort y=stepY;y<height-1;y+=stepY)
					matrix[y,x]=(matrix [y, x - halfX] + matrix [y - halfY, x] + matrix [y, x + halfX] + matrix [y + halfY, x]) / 4 + (Random.Range (-randRange * r, randRange * r));
		}
	}

	/// <summary>
	/// Создаёт реки.
	/// </summary>
	/// <param name="heightMatrix">Карта высот.</param>
	/// <param name="matrix">[out] Карта рек.</param>
	public void CreateRivers (float[,] heightMatrix, bool[,] matrix)
	{
		ushort height = (ushort)matrix.GetLength (0);
		ushort width = (ushort)matrix.GetLength (1);

		double avg = 0;
		foreach (float h in heightMatrix)
			avg += h;
		avg /= height * width;
		float minRiverHeight = (float)avg * RiversParam.Height;

		for (byte i=0; i<RiversParam.Count; ++i) 
		{
			bool riverCreated = false;
			for (ushort y=1; y<height - 1 && !riverCreated; ++y) 
				for (ushort x=1; x<width -1 && !riverCreated; ++x) 
					if (heightMatrix [y, x] > minRiverHeight && !matrix [y, x] && RiverNeighbours (y, x, matrix) == 0) //Проверяем, можно ли нам начать создание реки с этого хекса
						for (byte k=0; k<RiversParam.Attempts&&!riverCreated; ++k) 
						{
							RiverStack.Push (new Vector2 (x, y));
							DirectRiver (y, x, heightMatrix, matrix); //Запускаем рекурсию
							if (RiverStack.Count >= RiversParam.MinimumLength) 
							{ //Если река получилась больше необходим длины, то помечаем ячейки матрицы, иначе пробуем ещё раз 
								foreach (Vector2 hex in RiverStack)
									matrix [(int)hex.y, (int)hex.x] = true;
								riverCreated = true;
							}
							RiverStack.Clear ();
						}
		}
	}
	
	enum Direction
	{
		TOP_LEFT,
		BOTTOM_LEFT,
		TOP,
		BOTTOM,
		TOP_RIGHT,
		BOTTOM_RIGHT
	}

	/// <summary>
	/// Выбирает направление распространения реки.
	/// </summary>
	/// <param name="y">y координата.</param>
	/// <param name="x">x координата.</param>
	/// <param name="heightMatrix">Карта высот.</param>
	/// <param name="matrix">Карта рек.</param>
	void DirectRiver (ushort y, ushort x, float[,] heightMatrix, bool[,] matrix)
	{
		ushort height = (ushort)matrix.GetLength (0);
		ushort width = (ushort)matrix.GetLength (1);

		if (y > 0 && y < height - 1 && x > 0 && x < width - 1) 
		{
			byte limiter = 0; //Переменная, контролирующая проверку всех направлений и выход из цикла, если ни одно не подходит
			bool dirFound = false;

			byte k = (byte)((x % 2) != 0 ? 1 : 0); //Учитываем чётность/нечётность ряда хексов
			do 
			{
				switch (Random.Range (0, 7)) { //Выбираем случайное направление
				case (int)Direction.TOP_LEFT:
					if (heightMatrix [y - 1 + k, x - 1] * RiversParam.FlowHeightKoef <= heightMatrix [y, x] && !matrix [y - 1 + k, x - 1] && RiverNeighbours ((ushort)(y - 1 + k), (ushort)(x - 1), matrix) < 2) 
					{
						RiverStack.Push (new Vector2 (x - 1, y - 1 + k));
						DirectRiver ((ushort)(y - 1 + k), (ushort)(x - 1), heightMatrix, matrix);
						dirFound = true;
					}
					limiter++;
					break;
				case (int)Direction.BOTTOM_LEFT:
					if (heightMatrix [y + k, x - 1] * RiversParam.FlowHeightKoef <= heightMatrix [y, x] && !matrix [y + k, x - 1] && RiverNeighbours ((ushort)(y + k), (ushort)(x - 1), matrix) < 2) 
					{
						RiverStack.Push (new Vector2 (x - 1, y + k));
						DirectRiver ((ushort)(y + k), (ushort)(x - 1), heightMatrix, matrix);
						dirFound = true;
					} 
					limiter++;
					break;
				case (int)Direction.TOP:
					if (heightMatrix [y - 1, x] * RiversParam.FlowHeightKoef <= heightMatrix [y, x] && !matrix [y - 1, x] && RiverNeighbours ((ushort)(y - 1), x, matrix) < 2) 
					{
						RiverStack.Push (new Vector2 (x, y - 1));
						DirectRiver ((ushort)(y - 1), x, heightMatrix, matrix);
						dirFound = true;
					} 
					limiter++;
					break;
				case (int)Direction.BOTTOM:
					if (heightMatrix [y + 1, x] * RiversParam.FlowHeightKoef <= heightMatrix [y, x] && !matrix [y + 1, x] && RiverNeighbours ((ushort)(y - 1), x, matrix) < 2) 
					{
						RiverStack.Push (new Vector2 (x, y + 1));
						DirectRiver ((ushort)(y + 1), x, heightMatrix, matrix);
						dirFound = true;
					} 
					limiter++;
					break;
				case (int)Direction.TOP_RIGHT:
					if (heightMatrix [y - 1 + k, x + 1] * RiversParam.FlowHeightKoef <= heightMatrix [y, x] && !matrix [y - 1 + k, x + 1] && RiverNeighbours ((ushort)(y - 1 + k), (ushort)(x + 1), matrix) < 2) 
					{
						RiverStack.Push (new Vector2 (x + 1, y - 1 + k));
						DirectRiver ((ushort)(y - 1 + k), (ushort)(x + 1), heightMatrix, matrix);
						dirFound = true;
					} 
					limiter++;
					break;
				case (int)Direction.BOTTOM_RIGHT:
					if (heightMatrix [y + k, x + 1] * RiversParam.FlowHeightKoef <= heightMatrix [y, x] && !matrix [y + k, x + 1] && RiverNeighbours ((ushort)(y + k), (ushort)(x + 1), matrix) < 2) 
					{
						RiverStack.Push (new Vector2 (x + 1, y + k));
						DirectRiver ((ushort)(y + k), (ushort)(x + 1), heightMatrix, matrix);
						dirFound = true;
					}
					limiter++;
					break;
				}
			} 
			while(!dirFound&&limiter!=6);
		}
	}
	
	/// <summary>
	/// Подсчитывает сколько рядом "рек".
	/// </summary>
	/// <returns>Число хексов вокруг, занятых рекой.</returns>
	/// <param name="y"> y координата.</param>
	/// <param name="x"> x координата.</param>
	/// <param name="matrix">Карта рек.</param>
	/// Функция подсчитывает количство соседних клеток, помеченных как "Река" или находящихся в "стеке реки"
	/// TODO Проверить работу стека реки
	byte RiverNeighbours (ushort y, ushort x, bool[,] matrix)
	{
		ushort height = (ushort)matrix.GetLength (0);
		ushort width = (ushort)matrix.GetLength (1);

		byte k = (byte)((x % 2) != 0 ? 1 : 0);

		byte riversCount = 0;
		if (y > 0 && x > 0 && (matrix [y - 1 + k, x - 1] || RiverStack.Contains (new Vector2 (x - 1, y - 1 + k)))) 
			riversCount++;
		if (x > 0 && y < height - 1 && (matrix [y + k, x - 1] || RiverStack.Contains (new Vector2 (x - 1, y + k)))) 
			riversCount++;
		if (y > 0 && (matrix [y - 1, x] || RiverStack.Contains (new Vector2 (x, y - 1)))) 
			riversCount++;
		if (y < height - 1 && (matrix [y + 1, x] || RiverStack.Contains (new Vector2 (x, y + 1)))) 
			riversCount++;
		if (y > 0 && x < width - 1 && (matrix [y - 1 + k, x + 1] || RiverStack.Contains (new Vector2 (x + 1, y - 1 + k)))) 
			riversCount++;
		if (x < width - 1 && y < height - 1 && (matrix [y + k, x + 1] || RiverStack.Contains (new Vector2 (x + 1, y + k)))) 
			riversCount++;

		return riversCount;
	}
}
