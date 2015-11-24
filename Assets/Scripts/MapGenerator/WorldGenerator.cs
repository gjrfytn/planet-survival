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
	public ushort GlobalMapSize; //Должен быть 2 в n-ой степени
	public ushort LocalMapSize; //Должен быть 2 в n-ой степени
	
	public float LandscapeRoughness;
	public float ForestRoughness;
	public RiversParameters RiversParam;

	//TODO Проверить использование координат в RiverStack
	private Stack<Vector2> RiverStack = new Stack<Vector2> (); //Стек для постройки реки

	void Awake()
	{
		Debug.Assert (Mathf.IsPowerOfTwo (GlobalMapSize));
		Debug.Assert (Mathf.IsPowerOfTwo (LocalMapSize));
		
		GlobalMapSize++;
		LocalMapSize++;
	}	

	/// <summary>
	/// Создаёт карту высот. 
	/// </summary>
	/// <param name="matrix">[out] Карта высот.</param>
	/// <param name="r">Шероховатость.</param>
	/// Использует фрактальный алгоритм Diamond Square
	public void CreateHeightmap (float[,] matrix, float r)
	{
		ushort size = (ushort)matrix.GetLength (0);

		//Задаём начальные значения по углам
		matrix [0, 0] = Random.Range (0.0f, 1.0f); 
		matrix [0, size - 1] = Random.Range (0.0f, 1.0f);
		matrix [size - 1, 0] = Random.Range (0.0f, 1.0f);
		matrix [size - 1, size - 1] = Random.Range (0.0f, 1.0f);

		for (ushort step=(ushort)(size -1), half=(ushort)(step/2); half!=0; step/=2, half/=2) 
		{
			for (ushort y=half; y<size; y+=step)
				for (ushort x=half; x<size; x+=step)
					matrix [y, x] = (matrix [y - half, x - half] + matrix [y + half, x - half] + matrix [y - half, x + half] + matrix [y + half, x + half]) / 4 + (Random.Range ((-half / (float)size) * r, (half / (float)size) * r));

			for (ushort i=half; i<size; i+=step) 
			{
				matrix [0, i] = (0 + matrix [0, i - half] + matrix [0 + half, i] + matrix [0, i + half]) / 4 + (Random.Range ((-half / (float)size) * r, (half / (float)size) * r));
				matrix [size - 1, i] = (matrix [size - 1 - half, i] + matrix [size - 1, i - half] + 0 + matrix [size - 1, i + half]) / 4 + (Random.Range ((-half / (float)size) * r, (half / (float)size) * r));
				matrix [i, 0] = (matrix [i - half, 0] + 0 + matrix [i + half, 0] + matrix [i, 0 + half]) / 4 + (Random.Range ((-half / (float)size) * r, (half / (float)size) * r));
				matrix [i, size - 1] = (matrix [i - half, size - 1] + matrix [i, size  - 1 - half] + matrix [i + half, size - 1] + 0) / 4 + (Random.Range ((-half / (float)size) * r, (half / (float)size) * r));
			}

			for (ushort i=half; i<size -1; i+=step)
				for (ushort j=step; j<size -1; j+=step) 
				{
					matrix [j, i] = (matrix [j - half, i] + matrix [j, i - half] + matrix [j + half, i] + matrix [j, i + half]) / 4 + (Random.Range ((-half / (float)size) * r, (half / (float)size) * r));
					matrix [i, j] = (matrix [i, j - half] + matrix [i - half, j] + matrix [i, j + half] + matrix [i + half, j]) / 4 + (Random.Range ((-half / (float)size) * r, (half / (float)size) * r));
				}
		}
	}

	/// <summary>
	/// Создаёт реки.
	/// </summary>
	/// <param name="heightMatrix">Карта высот.</param>
	/// <param name="matrix">[out] Карта рек.</param>
	public void CreateRivers (float[,] heightMatrix, bool[,] matrix)
	{
		ushort size = (ushort)matrix.GetLength (0);

		double avg = 0;
		foreach (float height in heightMatrix)
			avg += height;
		avg /= size * size;
		float minRiverHeight = (float)avg * RiversParam.Height;

		for (byte i=0; i<RiversParam.Count; ++i) 
		{
			bool riverCreated = false;
			for (ushort y=1; y<size - 1 && !riverCreated; ++y) 
				for (ushort x=1; x<size -1 && !riverCreated; ++x) 
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
		ushort size = (ushort)matrix.GetLength (0);

		if (y > 0 && y < size - 1 && x > 0 && x < size - 1) 
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
		ushort size = (ushort)matrix.GetLength (0);

		byte k = (byte)((x % 2) != 0 ? 1 : 0);

		byte riversCount = 0;
		if (y > 0 && x > 0 && (matrix [y - 1 + k, x - 1] || RiverStack.Contains (new Vector2 (x - 1, y - 1 + k)))) 
			riversCount++;
		if (x > 0 && y < size - 1 && (matrix [y + k, x - 1] || RiverStack.Contains (new Vector2 (x - 1, y + k)))) 
			riversCount++;
		if (y > 0 && (matrix [y - 1, x] || RiverStack.Contains (new Vector2 (x, y - 1)))) 
			riversCount++;
		if (y < size - 1 && (matrix [y + 1, x] || RiverStack.Contains (new Vector2 (x, y + 1)))) 
			riversCount++;
		if (y > 0 && x < size - 1 && (matrix [y - 1 + k, x + 1] || RiverStack.Contains (new Vector2 (x + 1, y - 1 + k)))) 
			riversCount++;
		if (x < size - 1 && y < size - 1 && (matrix [y + k, x + 1] || RiverStack.Contains (new Vector2 (x + 1, y + k)))) 
			riversCount++;

		return riversCount;
	}
}
