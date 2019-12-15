using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class Estatus
{
    public double posX, posY;
    public string Qn; //representa el estado el cual fue ejecutado en determinada posX y posY

    public Estatus(double x, double y, string q)
    {
        posX = x;
        posY = y;
        Qn = q;
    }

    public Estatus(string element) //ejemplo:   element = "1 2 jump"
    {
        string []elements = element.Split(' ');
        posX = Convert.ToDouble(elements[0]);
        posY = Convert.ToDouble(elements[1]);
        Qn = elements[2].ToString();
    }

    public override string ToString()
    {
        return posX + " " + posY + " " + Qn;
    }

    public bool Equals(double x, double y)
    {
        double acX = Math.Round(x, 1, MidpointRounding.ToEven);
        double acY = Math.Round(y, 1, MidpointRounding.ToEven);
        double poX = Math.Round(posX, 1, MidpointRounding.ToEven);
        double poY = Math.Round(posY, 1, MidpointRounding.ToEven);

        if(acX == poX && acY == poY) // en caso que la posicion actual(x,y) coincida
            return true;
        
        return false;
    }
}