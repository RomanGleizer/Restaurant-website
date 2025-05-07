﻿namespace Restaurant.Api.Models;

public class MenuItem
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; } = null!;
    public string ImagePath { get; set; } = null!;
}