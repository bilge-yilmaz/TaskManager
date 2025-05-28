// Task Manager Application
class TaskManager {
    constructor() {
        this.apiBaseUrl = 'http://localhost:5027/api';
        this.token = localStorage.getItem('token');
        this.currentUser = JSON.parse(localStorage.getItem('currentUser') || 'null');
        this.currentPage = 1;
        this.pageSize = 10;
        this.totalPages = 1;
        
        this.init();
    }

    init() {
        this.setupEventListeners();
        this.checkAuthentication();
    }

    setupEventListeners() {
        // Auth events
        document.getElementById('loginForm').addEventListener('submit', (e) => this.handleLogin(e));
        document.getElementById('registerForm').addEventListener('submit', (e) => this.handleRegister(e));
        document.getElementById('toggleAuth').addEventListener('click', () => this.toggleAuthForm());
        document.getElementById('logoutBtn').addEventListener('click', () => this.logout());

        // Task events
        document.getElementById('addTaskForm').addEventListener('submit', (e) => this.handleAddTask(e));
        document.getElementById('editTaskForm').addEventListener('submit', (e) => this.handleEditTask(e));
        document.getElementById('saveTaskChanges').addEventListener('click', () => this.saveTaskChanges());
        document.getElementById('refreshTasks').addEventListener('click', () => this.loadTasks());

        // Filter events
        document.getElementById('applyFilters').addEventListener('click', () => this.applyFilters());
        document.getElementById('clearFilters').addEventListener('click', () => this.clearFilters());
        
        // Search on enter
        document.getElementById('searchTerm').addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                this.applyFilters();
            }
        });
    }

    async checkAuthentication() {
        if (this.token) {
            try {
                const response = await this.apiCall('/auth/validate-token', 'GET');
                // API direkt user bilgilerini döndürüyor
                if (response && response.valid) {
                    this.currentUser = {
                        userId: response.userId,
                        username: response.username,
                        email: response.email
                    };
                    localStorage.setItem('currentUser', JSON.stringify(this.currentUser));
                    this.showMainContent();
                    this.loadDashboard();
                    this.loadTasks();
                } else {
                    this.logout();
                }
            } catch (error) {
                console.error('Token validation failed:', error);
                this.logout();
            }
        } else {
            this.showAuthModal();
        }
    }

    async apiCall(endpoint, method = 'GET', data = null) {
        const config = {
            method,
            headers: {
                'Content-Type': 'application/json',
            }
        };

        if (this.token) {
            config.headers['Authorization'] = `Bearer ${this.token}`;
        }

        if (data) {
            config.body = JSON.stringify(data);
        }

        const response = await fetch(`${this.apiBaseUrl}${endpoint}`, config);
        
        if (!response.ok) {
            const errorData = await response.json().catch(() => ({ message: 'Network error' }));
            throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
        }

        return await response.json();
    }

    async handleLogin(e) {
        e.preventDefault();
        
        const username = document.getElementById('loginUsername').value;
        const password = document.getElementById('loginPassword').value;

        try {
            this.showLoading('loginForm');
            const response = await this.apiCall('/auth/login', 'POST', { 
                usernameOrEmail: username, 
                password: password 
            });
            
            if (response && response.token) {
                this.token = response.token;
                this.currentUser = {
                    username: response.username,
                    email: response.email,
                    firstName: response.firstName,
                    lastName: response.lastName
                };
                localStorage.setItem('token', this.token);
                localStorage.setItem('currentUser', JSON.stringify(this.currentUser));
                
                this.hideAuthModal();
                this.showMainContent();
                this.loadDashboard();
                this.loadTasks();
                this.showAlert('Başarıyla giriş yaptınız!', 'success');
            } else {
                throw new Error('Giriş başarısız: Token bulunamadı');
            }
        } catch (error) {
            console.error('Login error:', error);
            this.showAlert(error.message || 'Giriş işlemi başarısız', 'danger');
        } finally {
            this.hideLoading('loginForm');
        }
    }

    async handleRegister(e) {
        e.preventDefault();
        
        const userData = {
            firstName: document.getElementById('registerFirstName').value,
            lastName: document.getElementById('registerLastName').value,
            username: document.getElementById('registerUsername').value,
            email: document.getElementById('registerEmail').value,
            password: document.getElementById('registerPassword').value
        };

        try {
            this.showLoading('registerForm');
            const response = await this.apiCall('/auth/register', 'POST', userData);
            
            if (response && response.token) {
                this.showAlert('Kayıt başarılı! Şimdi giriş yapabilirsiniz.', 'success');
                this.toggleAuthForm();
                document.getElementById('registerForm').reset();
            } else {
                throw new Error('Kayıt başarısız: Geçersiz yanıt');
            }
        } catch (error) {
            console.error('Register error:', error);
            this.showAlert(error.message || 'Kayıt işlemi başarısız', 'danger');
        } finally {
            this.hideLoading('registerForm');
        }
    }

    async handleAddTask(e) {
        e.preventDefault();
        
        const taskData = {
            title: document.getElementById('taskTitle').value,
            description: document.getElementById('taskDescription').value,
            priority: parseInt(document.getElementById('taskPriority').value),
            category: parseInt(document.getElementById('taskCategory').value),
            dueDate: document.getElementById('taskDueDate').value || null,
            tags: document.getElementById('taskTags').value.split(',').map(tag => tag.trim()).filter(tag => tag)
        };

        try {
            this.showLoading('addTaskForm');
            const response = await this.apiCall('/tasks', 'POST', taskData);
            
            if (response && response.id) {
                this.showAlert('Görev başarıyla eklendi!', 'success');
                document.getElementById('addTaskForm').reset();
                this.loadDashboard();
                this.loadTasks();
            } else {
                throw new Error('Görev ekleme başarısız: Geçersiz yanıt');
            }
        } catch (error) {
            this.showAlert(error.message || 'Görev eklenirken hata oluştu', 'danger');
        } finally {
            this.hideLoading('addTaskForm');
        }
    }

    async handleEditTask(taskId) {
        try {
            const response = await this.apiCall(`/tasks/${taskId}`);
            
            if (response && response.id) {
                const task = response;
                
                document.getElementById('editTaskId').value = task.id;
                document.getElementById('editTaskTitle').value = task.title;
                document.getElementById('editTaskDescription').value = task.description || '';
                document.getElementById('editTaskPriority').value = task.priority;
                document.getElementById('editTaskCategory').value = task.category;
                document.getElementById('editTaskTags').value = task.tags ? task.tags.join(', ') : '';
                
                if (task.dueDate) {
                    const date = new Date(task.dueDate);
                    document.getElementById('editTaskDueDate').value = date.toISOString().slice(0, 16);
                }
                
                const modal = new bootstrap.Modal(document.getElementById('editTaskModal'));
                modal.show();
            } else {
                throw new Error('Görev bilgileri alınamadı');
            }
        } catch (error) {
            this.showAlert(error.message || 'Görev düzenleme hatası', 'danger');
        }
    }

    async saveTaskChanges() {
        const taskId = document.getElementById('editTaskId').value;
        const taskData = {
            title: document.getElementById('editTaskTitle').value,
            description: document.getElementById('editTaskDescription').value,
            priority: parseInt(document.getElementById('editTaskPriority').value),
            category: parseInt(document.getElementById('editTaskCategory').value),
            dueDate: document.getElementById('editTaskDueDate').value || null,
            tags: document.getElementById('editTaskTags').value.split(',').map(tag => tag.trim()).filter(tag => tag)
        };

        try {
            const response = await this.apiCall(`/tasks/${taskId}`, 'PUT', taskData);
            
            if (response && response.id) {
                this.showAlert('Görev başarıyla güncellendi!', 'success');
                const modal = bootstrap.Modal.getInstance(document.getElementById('editTaskModal'));
                modal.hide();
                this.loadDashboard();
                this.loadTasks();
            } else {
                throw new Error('Görev güncelleme başarısız');
            }
        } catch (error) {
            this.showAlert(error.message || 'Görev güncellenirken hata oluştu', 'danger');
        }
    }

    async toggleTaskComplete(taskId, isCompleted) {
        try {
            const endpoint = isCompleted ? `/tasks/${taskId}/uncomplete` : `/tasks/${taskId}/complete`;
            
            const response = await this.apiCall(endpoint, 'POST');
            
            if (response && response.message) {
                this.loadDashboard();
                this.loadTasks();
                this.showAlert(isCompleted ? 'Görev tamamlanmadı olarak işaretlendi' : 'Görev tamamlandı!', 'success');
            } else {
                throw new Error('Görev durumu değiştirilemedi');
            }
        } catch (error) {
            this.showAlert(error.message || 'Görev durumu değiştirilirken hata oluştu', 'danger');
        }
    }

    async deleteTask(taskId) {
        if (!confirm('Bu görevi silmek istediğinizden emin misiniz?')) {
            return;
        }

        try {
            const response = await this.apiCall(`/tasks/${taskId}`, 'DELETE');
            
            if (response && response.message) {
                this.showAlert('Görev başarıyla silindi!', 'success');
                this.loadDashboard();
                this.loadTasks();
            } else {
                throw new Error('Görev silinemedi');
            }
        } catch (error) {
            this.showAlert(error.message || 'Görev silinirken hata oluştu', 'danger');
        }
    }

    async loadDashboard() {
        try {
            const response = await this.apiCall('/tasks/stats');
            
            if (response && typeof response.totalTasks !== 'undefined') {
                document.getElementById('totalTasks').textContent = response.totalTasks;
                document.getElementById('completedTasks').textContent = response.completedTasks;
                document.getElementById('pendingTasks').textContent = response.pendingTasks;
                document.getElementById('overdueTasks').textContent = response.overdueTasks;
            } else {
                document.getElementById('totalTasks').textContent = '0';
                document.getElementById('completedTasks').textContent = '0';
                document.getElementById('pendingTasks').textContent = '0';
                document.getElementById('overdueTasks').textContent = '0';
            }
        } catch (error) {
            document.getElementById('totalTasks').textContent = '0';
            document.getElementById('completedTasks').textContent = '0';
            document.getElementById('pendingTasks').textContent = '0';
            document.getElementById('overdueTasks').textContent = '0';
        }
    }

    async loadTasks(page = 1) {
        try {
            this.currentPage = page;
            const params = new URLSearchParams({
                page: page,
                pageSize: this.pageSize
            });

            // Add filters
            const status = document.getElementById('filterStatus').value;
            const priority = document.getElementById('filterPriority').value;
            const category = document.getElementById('filterCategory').value;
            const search = document.getElementById('searchTerm').value;

            if (status !== '') params.append('isCompleted', status);
            if (priority) params.append('priority', priority);
            if (category) params.append('category', category);
            if (search) params.append('search', search);

            const response = await this.apiCall(`/tasks?${params}`);
            
            if (response && response.items) {
                this.renderTasks(response.items);
                this.renderPagination(response.totalPages, page);
                this.totalPages = response.totalPages;
            } else {
                this.renderTasks([]);
            }
        } catch (error) {
            this.showAlert(error.message || 'Görevler yüklenirken hata oluştu', 'danger');
            this.renderTasks([]);
        }
    }

    renderTasks(tasks) {
        const container = document.getElementById('tasksContainer');
        
        if (tasks.length === 0) {
            container.innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-tasks"></i>
                    <h5>Henüz görev yok</h5>
                    <p>Yeni bir görev ekleyerek başlayın!</p>
                </div>
            `;
            return;
        }

        container.innerHTML = tasks.map(task => this.renderTaskCard(task)).join('');
    }

    renderTaskCard(task) {
        const priorityClass = this.getPriorityClass(task.priority);
        const priorityText = this.getPriorityText(task.priority);
        const categoryText = this.getCategoryText(task.category);
        const dueDateClass = this.getDueDateClass(task.dueDate, task.isCompleted);
        
        const tagsHtml = task.tags && task.tags.length > 0 
            ? `<div class="task-tags">${task.tags.map(tag => `<span class="tag">${tag}</span>`).join('')}</div>`
            : '';

        const dueDateHtml = task.dueDate 
            ? `<small class="text-muted"><i class="fas fa-calendar me-1"></i>${this.formatDate(task.dueDate)}</small>`
            : '';

        return `
            <div class="card task-card ${priorityClass} ${task.isCompleted ? 'completed' : ''} ${dueDateClass}">
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-start mb-2">
                        <h6 class="task-title mb-0">${task.title}</h6>
                        <div class="d-flex gap-2">
                            <span class="priority-badge priority-${this.getPriorityClass(task.priority).replace('priority-', '')}">${priorityText}</span>
                            <span class="category-badge">${categoryText}</span>
                        </div>
                    </div>
                    
                    ${task.description ? `<p class="task-description">${task.description}</p>` : ''}
                    
                    <div class="task-meta d-flex justify-content-between align-items-center">
                        <div>
                            ${dueDateHtml}
                        </div>
                        <small class="text-muted">
                            <i class="fas fa-calendar-plus me-1"></i>${this.formatDate(task.createdAt)}
                        </small>
                    </div>
                    
                    ${tagsHtml}
                    
                    <div class="task-actions mt-3">
                        <button class="btn ${task.isCompleted ? 'btn-warning' : 'btn-success'} btn-sm" 
                                onclick="taskManager.toggleTaskComplete('${task.id}', ${task.isCompleted})">
                            <i class="fas ${task.isCompleted ? 'fa-undo' : 'fa-check'}"></i>
                            ${task.isCompleted ? 'Geri Al' : 'Tamamla'}
                        </button>
                        <button class="btn btn-outline-primary btn-sm" 
                                onclick="taskManager.handleEditTask('${task.id}')">
                            <i class="fas fa-edit"></i> Düzenle
                        </button>
                        <button class="btn btn-outline-danger btn-sm" 
                                onclick="taskManager.deleteTask('${task.id}')">
                            <i class="fas fa-trash"></i> Sil
                        </button>
                    </div>
                </div>
            </div>
        `;
    }

    renderPagination(totalPages, currentPage) {
        const pagination = document.getElementById('pagination');
        
        if (totalPages <= 1) {
            pagination.innerHTML = '';
            return;
        }

        let paginationHtml = '';
        
        // Previous button
        paginationHtml += `
            <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
                <a class="page-link" href="#" onclick="taskManager.loadTasks(${currentPage - 1}); return false;">
                    <i class="fas fa-chevron-left"></i>
                </a>
            </li>
        `;

        // Page numbers
        for (let i = 1; i <= totalPages; i++) {
            if (i === 1 || i === totalPages || (i >= currentPage - 2 && i <= currentPage + 2)) {
                paginationHtml += `
                    <li class="page-item ${i === currentPage ? 'active' : ''}">
                        <a class="page-link" href="#" onclick="taskManager.loadTasks(${i}); return false;">${i}</a>
                    </li>
                `;
            } else if (i === currentPage - 3 || i === currentPage + 3) {
                paginationHtml += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
        }

        // Next button
        paginationHtml += `
            <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
                <a class="page-link" href="#" onclick="taskManager.loadTasks(${currentPage + 1}); return false;">
                    <i class="fas fa-chevron-right"></i>
                </a>
            </li>
        `;

        pagination.innerHTML = paginationHtml;
    }

    applyFilters() {
        this.loadTasks(1);
    }

    clearFilters() {
        document.getElementById('filterStatus').value = '';
        document.getElementById('filterPriority').value = '';
        document.getElementById('filterCategory').value = '';
        document.getElementById('searchTerm').value = '';
        this.loadTasks(1);
    }

    // Utility methods
    getPriorityClass(priority) {
        const classes = {
            1: 'priority-low',
            2: 'priority-medium',
            3: 'priority-high',
            4: 'priority-critical'
        };
        return classes[priority] || 'priority-medium';
    }

    getPriorityText(priority) {
        const texts = {
            1: 'Düşük',
            2: 'Orta',
            3: 'Yüksek',
            4: 'Kritik'
        };
        return texts[priority] || 'Orta';
    }

    getCategoryText(category) {
        const texts = {
            1: 'Kişisel',
            2: 'İş',
            3: 'Sağlık',
            4: 'Eğitim',
            5: 'Finans',
            6: 'Alışveriş',
            7: 'Seyahat',
            8: 'Diğer'
        };
        return texts[category] || 'Diğer';
    }

    getDueDateClass(dueDate, isCompleted) {
        if (!dueDate || isCompleted) return '';
        
        const now = new Date();
        const due = new Date(dueDate);
        const diffTime = due - now;
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
        
        if (diffDays < 0) return 'overdue';
        if (diffDays === 0) return 'due-today';
        if (diffDays <= 3) return 'due-soon';
        
        return '';
    }

    formatDate(dateString) {
        const date = new Date(dateString);
        return date.toLocaleDateString('tr-TR', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    // UI methods
    showAuthModal() {
        const modal = new bootstrap.Modal(document.getElementById('authModal'));
        modal.show();
    }

    hideAuthModal() {
        try {
            const modalElement = document.getElementById('authModal');
            if (modalElement) {
                const modal = bootstrap.Modal.getInstance(modalElement);
                if (modal) {
                    modal.hide();
                } else {
                    const newModal = new bootstrap.Modal(modalElement);
                    newModal.hide();
                }
            }
        } catch (error) {
            console.error('Error hiding auth modal:', error);
        }
    }

    showMainContent() {
        const mainContent = document.getElementById('mainContent');
        const userInfo = document.getElementById('userInfo');
        const logoutBtn = document.getElementById('logoutBtn');
        const userName = document.getElementById('userName');
        
        if (mainContent) {
            mainContent.style.display = 'block';
        } else {
            console.error('Main content element not found');
        }
        
        if (userInfo) {
            userInfo.style.display = 'block';
        } else {
            console.error('User info element not found');
        }
        
        if (logoutBtn) {
            logoutBtn.style.display = 'block';
        } else {
            console.error('Logout button element not found');
        }
        
        if (this.currentUser && userName) {
            const userDisplayName = this.currentUser.firstName && this.currentUser.lastName 
                ? `${this.currentUser.firstName} ${this.currentUser.lastName}`
                : this.currentUser.username || 'Kullanıcı';
            userName.textContent = userDisplayName;
        } else {
            console.error('Current user or userName element not found');
        }
    }

    hideMainContent() {
        document.getElementById('mainContent').style.display = 'none';
        document.getElementById('userInfo').style.display = 'none';
        document.getElementById('logoutBtn').style.display = 'none';
    }

    toggleAuthForm() {
        const loginForm = document.getElementById('loginForm');
        const registerForm = document.getElementById('registerForm');
        const toggleBtn = document.getElementById('toggleAuth');
        const modalTitle = document.getElementById('authModalTitle');

        if (loginForm.style.display === 'none') {
            loginForm.style.display = 'block';
            registerForm.style.display = 'none';
            toggleBtn.textContent = 'Hesabın yok mu? Kayıt ol';
            modalTitle.textContent = 'Giriş Yap';
        } else {
            loginForm.style.display = 'none';
            registerForm.style.display = 'block';
            toggleBtn.textContent = 'Zaten hesabın var mı? Giriş yap';
            modalTitle.textContent = 'Kayıt Ol';
        }
    }

    showLoading(formId) {
        const form = document.getElementById(formId);
        const submitBtn = form.querySelector('button[type="submit"]');
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Yükleniyor...';
    }

    hideLoading(formId) {
        const form = document.getElementById(formId);
        const submitBtn = form.querySelector('button[type="submit"]');
        submitBtn.disabled = false;
        
        if (formId === 'loginForm') {
            submitBtn.innerHTML = 'Giriş Yap';
        } else if (formId === 'registerForm') {
            submitBtn.innerHTML = 'Kayıt Ol';
        } else if (formId === 'addTaskForm') {
            submitBtn.innerHTML = '<i class="fas fa-plus me-2"></i>Görev Ekle';
        }
    }

    showAlert(message, type = 'info') {
        const alertHtml = `
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        
        // Insert at the top of main content
        const mainContent = document.getElementById('mainContent');
        if (mainContent.style.display !== 'none') {
            mainContent.insertAdjacentHTML('afterbegin', alertHtml);
        } else {
            // Show in auth modal
            const authModal = document.querySelector('#authModal .modal-body');
            authModal.insertAdjacentHTML('afterbegin', alertHtml);
        }

        // Auto remove after 5 seconds
        setTimeout(() => {
            const alert = document.querySelector('.alert');
            if (alert) {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            }
        }, 5000);
    }

    logout() {
        this.token = null;
        this.currentUser = null;
        localStorage.removeItem('token');
        localStorage.removeItem('currentUser');
        this.hideMainContent();
        this.showAuthModal();
    }
}

// Initialize the application
const taskManager = new TaskManager(); 