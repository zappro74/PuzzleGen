# Naming Conventions

## PascalCase (ex: `PuzzlePiece`)
- Classes  
- Structs  
- Enums  
- Properties  
- Public fields 
- Methods  

## camelCase (ex: `pieceId`)
- Local variables  
- Method parameters  
- Loop variables  

## _camelCase (ex: `_pieceId`)
- Private fields  
- Private serialized fields in Unity  

## UPPER_CASE (ex: `MAX_PIECES`) 
- Constants 

## Boolean Naming
Use prefixes:
- `is` → `isDragging`, `isSelected`  
- `has` → `hasConnection`, `hasSnapped`  
- `can` → `canSnap`, `canRotate`  

## Enum Values (PascalCase)

```csharp
public enum EdgeType
{
    Flat,
    Extruded,
    Intruded
}
```

## File Names
- Match class name exactly (PascalCase)

Examples:
- `PuzzlePiece.cs`  
- `PuzzleGenerator.cs`  

## Unity GameObject Names (Hierarchy)
- PascalCase or readable words

Examples:
- `PuzzleBoard`  
- `PieceContainer`  
- `Piece_0`, `Piece_1`  

## Branch Naming (Git)
- `feature/...`  
- `fix/...`  
- `refactor/...`  

Examples:
- `feature/piece-generation`  
- `feature/snapping-system`  
- `fix/save-load-error`  

## Terminology (Consistency Rule)
- Use `Piece`, not `Tile`  
- Use `Group`, not `Island`  
- Use `Connection`, not `Link`  
- Use `Edge`, not `Side`
- will add to this if we find any more inconsistencies
