Vagrant.configure("2") do |config|

  config.vm.box = 'bento/ubuntu-22.04'
  config.vm.hostname = "dormcare-arm"

  config.vm.synced_folder ".", "/vagrant"

  # Forward SSH-like app port and common docker service ports used by docker-compose
  config.vm.network "forwarded_port", guest: 2200, host: 2200
  # Ports from Docker-compose.yml (SQL, DBs, monitoring, dashboards)
  [1433, 5432, 3306, 8080, 9411, 9090, 3000, 9000, 8086].each do |p|
    config.vm.network "forwarded_port", guest: p, host: p
  end

  config.vm.provider "virtualbox" do |vb|
    vb.memory = "4096"
    vb.cpus = 2
  end

  config.vm.provision "shell", path: "scripts/vagrant_provision.sh"
end
