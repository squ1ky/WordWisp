// Счетчики для уникальных индексов
let questionIndex = 0;
let answerIndexes = {};

// Инициализация при загрузке страницы
document.addEventListener('DOMContentLoaded', function() {
    const existingQuestions = document.querySelectorAll('.question-card');
    questionIndex = existingQuestions.length;
    
    existingQuestions.forEach((question, index) => {
        const answers = question.querySelectorAll('.answer-item');
        answerIndexes[index] = answers.length;
    });
    
    updateQuestionNumbers();
});

// Добавление нового вопроса
function addQuestion() {
    const container = document.getElementById('questionsContainer');
    const questionHtml = createQuestionHtml(questionIndex);
    
    const questionDiv = document.createElement('div');
    questionDiv.innerHTML = questionHtml;
    questionDiv.className = 'question-card';
    questionDiv.setAttribute('data-question-index', questionIndex);
    
    container.appendChild(questionDiv);
    
    // Инициализируем счетчик ответов для нового вопроса
    answerIndexes[questionIndex] = 2; // Начинаем с 2 ответов
    
    questionIndex++;
    updateQuestionNumbers();
    updateFormIndexes();
    
    // Скрываем empty state если он есть
    const emptyState = document.querySelector('.empty-questions');
    if (emptyState) {
        emptyState.style.display = 'none';
    }
}

// Удаление вопроса
function removeQuestion(button) {
    const questionCard = button.closest('.question-card');
    const questionIdx = questionCard.getAttribute('data-question-index');
    
    delete answerIndexes[questionIdx];
    questionCard.remove();
    
    updateQuestionNumbers();
    updateFormIndexes();
    
    // Показываем empty state если вопросов нет
    const remainingQuestions = document.querySelectorAll('.question-card');
    if (remainingQuestions.length === 0) {
        showEmptyQuestionsState();
    }
}

// Добавление нового ответа
function addAnswer(button) {
    const questionCard = button.closest('.question-card');
    const questionIdx = questionCard.getAttribute('data-question-index');
    const answersContainer = questionCard.querySelector('.answers-container');
    
    const currentAnswerIndex = answerIndexes[questionIdx] || 0;
    const answerHtml = createAnswerHtml(questionIdx, currentAnswerIndex);
    
    const answerDiv = document.createElement('div');
    answerDiv.innerHTML = answerHtml;
    answerDiv.className = 'answer-item';
    answerDiv.setAttribute('data-answer-index', currentAnswerIndex);
    
    answersContainer.appendChild(answerDiv);
    
    answerIndexes[questionIdx] = currentAnswerIndex + 1;
    updateFormIndexes();
}

// Удаление ответа
function removeAnswer(button) {
    const answerItem = button.closest('.answer-item');
    const questionCard = button.closest('.question-card');
    const answersContainer = questionCard.querySelector('.answers-container');
    
    const remainingAnswers = answersContainer.querySelectorAll('.answer-item');
    if (remainingAnswers.length <= 2) {
        alert('Должно быть минимум 2 варианта ответа');
        return;
    }
    
    answerItem.remove();
    updateFormIndexes();
}

// Создание HTML для нового вопроса
function createQuestionHtml(qIndex) {
    return `
        <div class="question-header">
            <h4>Вопрос ${qIndex + 1}</h4>
            <button type="button" class="btn btn-danger btn-sm" onclick="removeQuestion(this)">
                <i class="bi bi-trash"></i>
            </button>
        </div>
        <div class="question-content">
            <div class="row">
                <div class="col-md-8">
                    <div class="form-group">
                        <label class="form-label">Текст вопроса *</label>
                        <textarea name="Input.Questions[${qIndex}].Question" class="form-control" rows="2" placeholder="Введите текст вопроса"></textarea>
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label class="form-label">Баллы *</label>
                        <input name="Input.Questions[${qIndex}].Points" type="number" class="form-control" min="1" max="100" value="1" />
                    </div>
                </div>
                <div class="col-md-2">
                    <div class="form-group">
                        <label class="form-label">Порядок</label>
                        <input name="Input.Questions[${qIndex}].Order" type="number" class="form-control" min="0" value="${qIndex}" />
                    </div>
                </div>
            </div>
            
            <div class="row">
                <div class="col-md-6">
                    <div class="form-group">
                        <label class="form-label">Изображение (URL)</label>
                        <input name="Input.Questions[${qIndex}].QuestionImagePath" class="form-control" placeholder="Ссылка на изображение" />
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="form-group">
                        <label class="form-label">Аудио (URL)</label>
                        <input name="Input.Questions[${qIndex}].QuestionAudioPath" class="form-control" placeholder="Ссылка на аудио" />
                    </div>
                </div>
            </div>
            
            <div class="answers-section">
                <div class="answers-header">
                    <label class="form-label">Варианты ответов</label>
                    <button type="button" class="btn btn-secondary btn-sm" onclick="addAnswer(this)">
                        <i class="bi bi-plus"></i> Добавить ответ
                    </button>
                </div>
                <div class="answers-container">
                    ${createAnswerHtml(qIndex, 0)}
                    ${createAnswerHtml(qIndex, 1)}
                </div>
            </div>
        </div>
    `;
}

// Создание HTML для нового ответа
function createAnswerHtml(questionIdx, answerIdx) {
    return `
        <div class="answer-item" data-answer-index="${answerIdx}">
            <div class="answer-content">
                <div class="answer-checkbox">
                    <input name="Input.Questions[${questionIdx}].Answers[${answerIdx}].IsCorrect" type="checkbox" class="form-check-input" value="true" />
                    <input name="Input.Questions[${questionIdx}].Answers[${answerIdx}].IsCorrect" type="hidden" value="false" />
                    <label class="form-check-label">Правильный</label>
                </div>
                <div class="answer-text">
                    <input name="Input.Questions[${questionIdx}].Answers[${answerIdx}].AnswerText" class="form-control" placeholder="Текст ответа" />
                </div>
                <div class="answer-image">
                    <input name="Input.Questions[${questionIdx}].Answers[${answerIdx}].AnswerImagePath" class="form-control" placeholder="URL изображения" />
                </div>
                <div class="answer-order">
                    <input name="Input.Questions[${questionIdx}].Answers[${answerIdx}].Order" type="number" class="form-control" min="0" placeholder="Порядок" value="${answerIdx}" />
                </div>
                <div class="answer-actions">
                    <button type="button" class="btn btn-danger btn-sm" onclick="removeAnswer(this)">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>
            </div>
        </div>
    `;
}

// Обновление нумерации вопросов
function updateQuestionNumbers() {
    const questions = document.querySelectorAll('.question-card');
    questions.forEach((question, index) => {
        const header = question.querySelector('.question-header h4');
        if (header) {
            header.textContent = `Вопрос ${index + 1}`;
        }
    });
}

// Обновление индексов в именах полей формы
function updateFormIndexes() {
    const questions = document.querySelectorAll('.question-card');
    
    questions.forEach((question, qIndex) => {
        question.setAttribute('data-question-index', qIndex);
        
        const questionInputs = question.querySelectorAll('input[name*="Questions"], textarea[name*="Questions"]');
        questionInputs.forEach(input => {
            const name = input.getAttribute('name');
            if (name) {
                const newName = name.replace(/Questions\[\d+\]/, `Questions[${qIndex}]`);
                input.setAttribute('name', newName);
            }
        });
        
        const answers = question.querySelectorAll('.answer-item');
        answers.forEach((answer, aIndex) => {
            answer.setAttribute('data-answer-index', aIndex);
            
            const answerInputs = answer.querySelectorAll('input[name*="Answers"]');
            answerInputs.forEach(input => {
                const name = input.getAttribute('name');
                if (name) {
                    let newName = name.replace(/Questions\[\d+\]/, `Questions[${qIndex}]`);
                    newName = newName.replace(/Answers\[\d+\]/, `Answers[${aIndex}]`);
                    input.setAttribute('name', newName);
                }
            });
        });
    });
}

// Показать состояние "нет вопросов"
function showEmptyQuestionsState() {
    const container = document.getElementById('questionsContainer');
    container.innerHTML = `
        <div class="empty-questions">
            <i class="bi bi-question-circle"></i>
            <p>Добавьте вопросы для упражнения</p>
            <button type="button" class="btn btn-primary" onclick="addQuestion()">
                <i class="bi bi-plus"></i> Добавить первый вопрос
            </button>
        </div>
    `;
}

// Валидация формы перед отправкой
document.getElementById('exerciseForm').addEventListener('submit', function(e) {
    const questions = document.querySelectorAll('.question-card');
    
    if (questions.length === 0) {
        e.preventDefault();
        alert('Добавьте хотя бы один вопрос');
        return;
    }
    
    let hasErrors = false;
    
    questions.forEach((question, qIndex) => {
        const questionText = question.querySelector('textarea[name*="Question"]');
        if (!questionText || !questionText.value.trim()) {
            hasErrors = true;
            alert(`Заполните текст вопроса ${qIndex + 1}`);
            return;
        }
        
        const answers = question.querySelectorAll('.answer-item');
        if (answers.length < 2) {
            hasErrors = true;
            alert(`Добавьте минимум 2 ответа для вопроса ${qIndex + 1}`);
            return;
        }
        
        let hasCorrectAnswer = false;
        let hasAnswerText = false;
        
        answers.forEach(answer => {
            const isCorrect = answer.querySelector('input[type="checkbox"]');
            const answerText = answer.querySelector('input[name*="AnswerText"]');
            
            if (isCorrect && isCorrect.checked) {
                hasCorrectAnswer = true;
            }
            
            if (answerText && answerText.value.trim()) {
                hasAnswerText = true;
            }
        });
        
        if (!hasCorrectAnswer) {
            hasErrors = true;
            alert(`Отметьте правильный ответ для вопроса ${qIndex + 1}`);
            return;
        }
        
        if (!hasAnswerText) {
            hasErrors = true;
            alert(`Заполните текст ответов для вопроса ${qIndex + 1}`);
            return;
        }
    });
    
    if (hasErrors) {
        e.preventDefault();
    }
});
