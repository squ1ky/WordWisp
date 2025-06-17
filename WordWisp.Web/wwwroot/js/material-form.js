document.addEventListener('DOMContentLoaded', function() {
    const typeRadios = document.querySelectorAll('input[name="Input.MaterialType"]');
    const contentSections = {
        'Text': document.getElementById('textContent'),
        'Image': document.getElementById('fileContent'),
        'Audio': document.getElementById('fileContent'),
        'Video': document.getElementById('videoContent')
    };
    
    const fileInput = document.getElementById('fileInput');
    const fileUploadArea = document.getElementById('fileUploadArea');
    const filePreview = document.getElementById('filePreview');
    const fileRequirements = document.querySelector('.file-requirements');

    // Обработка выбора типа материала
    typeRadios.forEach(radio => {
        radio.addEventListener('change', function() {
            updateContentSection(this.value);
        });
    });

    // Инициализация при загрузке
    const checkedType = document.querySelector('input[name="Input.MaterialType"]:checked');
    if (checkedType) {
        updateContentSection(checkedType.value);
    }

    function updateContentSection(type) {
        // Скрываем все секции
        Object.values(contentSections).forEach(section => {
            if (section) section.style.display = 'none';
        });

        // Показываем нужную секцию
        const targetSection = contentSections[type];
        if (targetSection) {
            targetSection.style.display = 'block';
        }

        // Обновляем требования к файлу
        updateFileRequirements(type);
    }

    function updateFileRequirements(type) {
        if (!fileRequirements) return;

        const requirements = {
            'Image': 'JPG, PNG, GIF, WebP до 5 МБ',
            'Audio': 'MP3, WAV, OGG, MP4 до 50 МБ'
        };

        fileRequirements.textContent = requirements[type] || '';
    }

    // Обработка загрузки файлов
    if (fileInput && fileUploadArea) {
        // Drag & Drop
        fileUploadArea.addEventListener('dragover', function(e) {
            e.preventDefault();
            this.classList.add('dragover');
        });

        fileUploadArea.addEventListener('dragleave', function(e) {
            e.preventDefault();
            this.classList.remove('dragover');
        });

        fileUploadArea.addEventListener('drop', function(e) {
            e.preventDefault();
            this.classList.remove('dragover');
            
            const files = e.dataTransfer.files;
            if (files.length > 0) {
                fileInput.files = files;
                showFilePreview(files[0]);
            }
        });

        // Выбор файла
        fileInput.addEventListener('change', function() {
            if (this.files.length > 0) {
                showFilePreview(this.files[0]);
            }
        });
    }

    function showFilePreview(file) {
        if (!filePreview) return;

        const fileName = filePreview.querySelector('.file-name');
        const fileSize = filePreview.querySelector('.file-size');
        const placeholder = document.querySelector('.file-upload-placeholder');

        if (fileName) fileName.textContent = file.name;
        if (fileSize) fileSize.textContent = formatFileSize(file.size);

        if (placeholder) placeholder.style.display = 'none';
        filePreview.style.display = 'flex';
    }

    function formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    // Глобальная функция для удаления файла
    window.removeFile = function() {
        if (fileInput) fileInput.value = '';
        if (filePreview) filePreview.style.display = 'none';
        
        const placeholder = document.querySelector('.file-upload-placeholder');
        if (placeholder) placeholder.style.display = 'block';
    };

    // Валидация формы
    const form = document.getElementById('materialForm');
    if (form) {
        form.addEventListener('submit', function(e) {
            const checkedType = document.querySelector('input[name="Input.MaterialType"]:checked');
            if (!checkedType) {
                e.preventDefault();
                alert('Пожалуйста, выберите тип материала');
                return;
            }

            const type = checkedType.value;
            
            // Проверяем обязательные поля в зависимости от типа
            if (type === 'Text') {
                const content = document.querySelector('textarea[name="Input.Content"]');
                if (!content || !content.value.trim()) {
                    e.preventDefault();
                    alert('Пожалуйста, введите содержимое текстового материала');
                    return;
                }
            } else if (type === 'Video') {
                const url = document.querySelector('input[name="Input.ExternalUrl"]');
                if (!url || !url.value.trim()) {
                    e.preventDefault();
                    alert('Пожалуйста, введите ссылку на видео');
                    return;
                }
            } else if (type === 'Image' || type === 'Audio') {
                if (!fileInput.files.length) {
                    e.preventDefault();
                    alert('Пожалуйста, выберите файл');
                    return;
                }
            }
        });
    }
});
