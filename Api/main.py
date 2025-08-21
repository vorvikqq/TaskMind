from fastapi import FastAPI
import pandas as pd
import joblib
from pydantic import BaseModel
from tensorflow.keras.models import load_model
from tensorflow.keras.losses import MeanSquaredError

app = FastAPI()

model = load_model("ml_assets/task_assignment_model.h5", custom_objects={"mae": MeanSquaredError()})
scaler = joblib.load("ml_assets/scaler.pkl")
mlb = joblib.load("ml_assets/mlb.pkl")
feature_names = joblib.load("ml_assets/feature_names.pkl")

class TaskInput(BaseModel):
    task: dict
    developers: list[dict]

def find_best_developer(task, developers):
    task_data = {
        "Difficulty": [task["difficulty"]],
        "DeadlineDays": [task["deadlineDays"]],
        "EstimatedHours": [task["estimatedHours"]],
        "RequiredSkills": [task["requiredSkills"]],
    }

    required_skills_encoded = pd.DataFrame(
        mlb.transform(task_data["RequiredSkills"]),
        columns=[f"Req_{s}" for s in mlb.classes_]
    )

    best_score = -1
    best_developer = None

    for dev in developers:
        dev_data = {
            "CurrentWorkload": [dev["currentWorkload"]],
            "TaskCompletionSpeed": [dev["taskCompletionSpeed"]],
            "DeveloperSkills": [dev["developerSkills"]],
        }

        developer_skills_encoded = pd.DataFrame(
            mlb.transform(dev_data["DeveloperSkills"]),
            columns=[f"Dev_{s}" for s in mlb.classes_]
        )

        input_data = pd.DataFrame(task_data).drop(columns=["RequiredSkills"])
        input_data = input_data.assign(CurrentWorkload=dev_data["CurrentWorkload"])
        input_data = input_data.assign(TaskCompletionSpeed=dev_data["TaskCompletionSpeed"])
        input_data = pd.concat([input_data, required_skills_encoded, developer_skills_encoded], axis=1)

        num_features = ["Difficulty", "DeadlineDays", "EstimatedHours", "CurrentWorkload", "TaskCompletionSpeed"]
        input_data[num_features] = scaler.transform(input_data[num_features])

        input_data = input_data.reindex(columns=feature_names, fill_value=0)

        score = model.predict(input_data)[0, 0]

        if score > best_score:
            best_score = score
            best_developer = dev

    return best_developer

@app.post("/predict")
def predict(data: TaskInput):
    best_dev = find_best_developer(data.task, data.developers)
    if best_dev is None:
        return {"BestDeveloper": None}
    return {
        "BestDeveloper": {
            "DeveloperID": best_dev["developerID"],  
            "DeveloperSkills": best_dev["developerSkills"], 
            "CurrentWorkload": best_dev["currentWorkload"],  
            "TaskCompletionSpeed": best_dev["taskCompletionSpeed"]
        }
    }
