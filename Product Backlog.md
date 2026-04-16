# Product Backlog

## High Priority

- The application opens a PNG or JPG image selected by the user from disk.
- The application rejects unsupported file types and displays an error message.
- The application displays the selected image before puzzle generation begins.
- The user can enter a target row count >= 2 for puzzle generation.
- The user can enter a target column count >= 2 for puzzle generation.
- The application rejects any input less than 2 for both row and column values.
- The application generates exactly (rows × columns) puzzle pieces.
- Every generated piece has a unique identifier.
- Every generated piece stores its current position.
- Every generated piece stores its current rotation value.
- Each piece renders only the portion of the source image corresponding to its grid position.
- The application displays all generated pieces after generation.

## Piece Shape Generation

- The application supports rectangular-grid puzzle generation.
- Each piece contains four edge definitions: top, right, bottom, and left.
- Each edge is classified as flat, extruded tab, or intruded tab.
- Each internal shared edge is assigned exactly one connector pair.
- Adjacent pieces use inverse connector types on shared edges.
- Border edges on the outer frame are flat.
- The application generates piece outlines as continuous closed paths.
- The application prevents self-intersecting piece outlines.

## Interaction

- The user can click a piece to select it.
- The user can drag a selected piece with the mouse.
- The selected piece follows the mouse while dragging.
- Releasing the mouse places the piece at its current location.
- Pieces cannot be dragged outside the board’s valid coordinate bounds.
- A dragged piece renders above non-selected pieces.

### Snapping & Connections
- The application determines valid neighboring pieces from each piece’s row and column.
- The application determines the correct relative alignment for each valid neighboring piece.
- The application detects when two valid neighboring pieces are within a defined snap tolerance.
- The application verifies that pieces are valid neighbors before snapping.
- The application aligns pieces to their correct relative positions when snapped.
- The application stores a connection between snapped pieces.
- A piece cannot form duplicate connections with the same neighbor.


## Group System (Islands)

- Connected pieces form a group.
- Each piece belongs to exactly one group.
- If pieces are connected to the correct piece(s):
  - Dragging any piece in a group moves all pieces in that group.
  - When two connected pieces belong to different groups, their groups merge.
- The application maintains correct group membership after each connection.

## Win Condition

- The application detects when all pieces belong to a single group.
- The application declares the puzzle solved when all pieces belong to one group.

## Gameplay Usability

- The user can preview the full reference image while playing.
- The user can hide the reference image while playing.
- The user can reset the current puzzle without selecting a new image.
- The user can reshuffle the current puzzle.
- The application includes a new game button.
- The application includes a restart button.
- The application includes a quit button.
- The application displays elapsed solve time.
- The application pauses or stops the timer when the puzzle is completed.

## Medium Priority

### Rotation

- The user can rotate a selected piece clockwise.
- The user can rotate a selected piece counterclockwise.
- Rotated pieces maintain correct collision bounds.
- Rotated pieces maintain correct snap detection.
- Pieces only snap when rotation is correct.
- The application allows rotation to be enabled or disabled before puzzle generation via difficulty choice.

### Save / Load
- The application saves the puzzle state to a JSON file.
- The saved JSON includes:
  - image reference
  - row and column values
  - generation seed
  - shuffle seed
  - piece positions
  - piece rotations
  - active connections
- The application can load a previously saved puzzle session.
- The regenerated puzzle definition matches the original puzzle definition for the same seeds.
- The application recalculates groups from the restored active connections.
- The application rejects malformed or invalid save files.


### Architecture Requirements

- The project uses separation between model, view, and controller responsibilities.
- Puzzle state is stored separately from rendering logic.
- Rendering logic is separated from gameplay logic.
- User input is handled through event-driven callbacks.
- Serialization logic is separate from gameplay logic.
- At least one feature uses functional-style operations.
- At least one operation uses asynchronous or background execution.
- The UI remains responsive during puzzle generation.
- The UI remains responsive while loading a saved puzzle.

## Lower Priority

### Polish

- The application plays a sound when pieces snap together.
- The application plays a sound when the puzzle is completed.
- The application supports difficulty presets.

#### Easy Mode
- Disables rotation
- High snap tolerance
- Pieces cannot snap to an incorrect piece

#### Medium Mode
- Enables rotation
- Rotates a small amount of pieces when shuffling
- Reduced snap tolerance
- Pieces can snap to another incorrect piece

#### Hard Mode
- Even more reduced snap tolerance
- Rotates every piece when shuffling
- Groups can break apart if dragged too fast (based on group size)

- The application displays a thumbnail preview before generation.
- The user can change background color or theme.
- The user can zoom and pan the puzzle board.
- The application supports keyboard shortcuts for common actions.
- The application logs runtime errors.
- The application includes a help or instructions screen.
- The application includes a credits screen.

## Stretch Goals

### Group Physics (Hard Mode)

- Hard mode enables collision-based interaction between groups.
- The application detects collisions between groups.
- The application calculates collision force between groups.
- The application breaks connections when collision force exceeds a threshold.
- The application recalculates groups after connections break.

### Advanced Features

- The application generates identical puzzle definitions when given the same generation seed.
- The application generates different layouts when given different random seeds.
  - This may require 2 different seeds
    - Generation seed
      - Piece edge pattern
      - Connector variations
    - Shuffle seed
      - Shuffles pieces in a new order while maintaining the same pieces
- The user can regenerate the puzzle with a new seed without restarting the application.
- The application supports irregular piece generation modes.
  - Possibly allows pieces to have more than one connector pair
- The application supports custom random seeds.
- The application tracks best completion time.
- Online Scoreboard

## Design Notes (not backlog requirements)
- Piece rendering uses UV mapping to display the correct portion of the source image within each generated piece shape.
- Edge assignment uses a deterministic seed-based generation method.
- Piece variation uses a deterministic seed-based function that returns the same output for the same input seed.