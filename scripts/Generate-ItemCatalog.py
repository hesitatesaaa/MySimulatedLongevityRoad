"""Build the compile-time item catalog from the supplied DOCX database."""
from __future__ import annotations

import glob
import json
import pathlib
import xml.etree.ElementTree as ET
import zipfile

ROOT = pathlib.Path(__file__).resolve().parents[1]
DOCS = [path for path in glob.glob(r"C:\Users\Arthur.lu\Downloads\*.docx")
        if pathlib.Path(path).stat().st_size == 47195]
if len(DOCS) != 1:
    raise SystemExit("Expected exactly one supplied item database DOCX")

with open(DOCS[0], "rb") as file, zipfile.ZipFile(file) as archive:
    document = ET.fromstring(archive.read("word/document.xml"))

w = "{http://schemas.openxmlformats.org/wordprocessingml/2006/main}"


def rows(table):
    return [["".join(node.text or "" for node in cell.iter(w + "t"))
             for cell in row.findall(w + "tc")]
            for row in table.findall(w + "tr")]


tables = [rows(table) for table in document.iter(w + "tbl")]
grades = {"黄级": 1, "玄级": 2, "地级": 3, "天级": 4}
categories = {"D": "Pill", "F": "Talisman", "B": "Artifact"}
prices = {"D": [20, 60, 180, 540], "F": [12, 36, 108, 324], "B": [30, 90, 270, 810]}
material_ids = {r[1]: r[0] for table in tables[2:5] for r in table[1:]}
material_ids["灵符纸"] = "F01"
items = []
for table_index in (2, 3, 5, 6, 7):
    for row in tables[table_index][1:]:
        if table_index in (2, 3):
            item_id, name = row[:2]
            category = "Plant" if name in {"灵虚草", "长生药材", "护脉草"} else "TalismanMaterial" if item_id == "F01" else "SpiritObject"
            price = 6 if item_id in {"A06", "A07", "A08", "F01"} else 24 if item_id in {"A04", "A05"} else 12
            items.append((item_id, name, category, 0, row[3], "", "", price))
        else:
            item_id, name = row[:2]
            prefix = item_id[0]
            grade = grades[row[3]]
            effect = row[4].replace("使用后消耗", "使用时消耗耐久") if item_id == "B001" else row[4]
            ingredients = [part.strip() for part in row[5].split("+")]
            if len(ingredients) != 2:
                raise ValueError((item_id, ingredients))
            items.append((item_id, name, categories[prefix], grade, effect,
                          material_ids[ingredients[0]], material_ids[ingredients[1]], prices[prefix][grade - 1]))

if len(items) != 38:
    raise ValueError(f"Expected 38 entries, found {len(items)}")

out = ROOT / "code" / "MySimulatedLongevityRoad" / "Data" / "MclslItemCatalog.cs"
header = '''using System;
using System.Collections.Generic;

namespace MySimulatedLongevityRoad.Data;

internal sealed class MclslItemDefinition
{
    internal readonly string Id;
    internal readonly string Name;
    internal readonly string Category;
    internal readonly int Grade;
    internal readonly string EffectText;
    internal readonly string IngredientA;
    internal readonly string IngredientB;
    internal readonly int Price;
    internal string IconPath => "ui/Items/" + Id;

    internal MclslItemDefinition(string id, string name, string category, int grade,
        string effectText, string ingredientA, string ingredientB, int price)
    {
        Id = id;
        Name = name;
        Category = category;
        Grade = grade;
        EffectText = effectText;
        IngredientA = ingredientA;
        IngredientB = ingredientB;
        Price = price;
    }
}

internal static class MclslItemCatalog
{
    internal static readonly MclslItemDefinition[] All =
    {
'''
footer = '''    };

    private static readonly Dictionary<string, MclslItemDefinition> ById = BuildIndex();

    internal static MclslItemDefinition Get(string id) =>
        !string.IsNullOrWhiteSpace(id) && ById.TryGetValue(id, out var item) ? item : null;

    private static Dictionary<string, MclslItemDefinition> BuildIndex()
    {
        Dictionary<string, MclslItemDefinition> index = new(StringComparer.Ordinal);
        foreach (MclslItemDefinition item in All) index.Add(item.Id, item);
        return index;
    }
}
'''
lines = ["        new(" + ", ".join(json.dumps(value, ensure_ascii=False) if isinstance(value, str) else str(value)
                             for value in item) + "),\n" for item in items]
out.write_text(header + "".join(lines) + footer, encoding="utf-8")
print(f"Generated {len(items)} entries in {out}")
