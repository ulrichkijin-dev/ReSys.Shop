@echo off
echo ===================================================
echo Starting Fashion Image Search API (Thesis Mode)
echo Default Search: DINOv2 | Default Recs: Fashion-CLIP
echo ===================================================

:: Check if virtual environment exists and activate it
if exist .venv\Scripts\activate (
    call .venv\Scripts\activate
) else if exist venv\Scripts\activate (
    call venv\Scripts\activate
)

:: Run the API using uvicorn
python -m uvicorn app.main:app --host 127.0.0.1 --port 8000 --reload

pause
